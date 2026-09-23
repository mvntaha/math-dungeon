using System;
using System.Collections.Generic;
using MathDungeon.Challenges;
using MathDungeon.Core;
using MathDungeon.Data;
using UnityEngine;

namespace MathDungeon.Dungeon
{
    /// <summary>
    /// Owns one dungeon's completion rules (SRS FR3 Dungeon Level Management).
    /// It watches challenges being solved, and once all four of this dungeon's
    /// challenges are done it marks the dungeon complete, which unlocks the next
    /// one in the linear chain and auto-saves.
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonManager : MonoBehaviour
    {
        [Tooltip("Which dungeon this scene is. 1, 2 or 3.")]
        [SerializeField] private int dungeonId = GameConstants.FirstDungeonId;

        [Tooltip("Found in the scene when left empty.")]
        [SerializeField] private ChallengeManager challengeManager;

        /// <summary>Raised with the solved count each time it changes, for the HUD.</summary>
        public event Action<int, int> ProgressChanged;

        /// <summary>Raised once when every challenge in this dungeon is solved.</summary>
        public event Action<int> DungeonCompleted;

        public int DungeonId => dungeonId;

        public int SolvedCount
        {
            get
            {
                DungeonProgress progress = Progress;
                return progress != null ? CountSolvedHere(progress) : 0;
            }
        }

        public bool IsComplete => SolvedCount >= GameConstants.ChallengesPerDungeon;

        private DungeonProgress Progress =>
            GameManager.Instance != null && GameManager.Instance.HasActiveProfile
                ? GameManager.Instance.ActiveProfile.GetProgress(dungeonId)
                : null;

        private void Awake()
        {
            if (!GameConstants.IsValidDungeonId(dungeonId))
            {
                Debug.LogError($"[DungeonManager] dungeonId {dungeonId} is outside 1-{GameConstants.LastDungeonId}.", this);
            }

            if (challengeManager == null)
            {
                challengeManager = FindFirstObjectByType<ChallengeManager>();
            }
        }

        private bool announcedCompletion;

        private void OnEnable()
        {
            if (challengeManager != null)
            {
                challengeManager.ChallengeSolved += OnChallengeSolved;
            }
        }

        private void OnDisable()
        {
            if (challengeManager != null)
            {
                challengeManager.ChallengeSolved -= OnChallengeSolved;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActiveProfileChanged -= OnActiveProfileChanged;
            }
        }

        private void Start()
        {
            // The profile may not be loaded yet - Start order between this and
            // whatever sets the profile is not guaranteed - so re-check whenever a
            // profile arrives as well as right now.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActiveProfileChanged += OnActiveProfileChanged;
            }

            RefreshCompletionState();
        }

        private void OnActiveProfileChanged(PlayerProfile profile)
        {
            RefreshCompletionState();
        }

        /// <summary>
        /// Re-reads progress and announces completion if this dungeon is already
        /// finished, so a dungeon resumed from a save opens its gate on arrival.
        /// </summary>
        private void RefreshCompletionState()
        {
            ProgressChanged?.Invoke(SolvedCount, GameConstants.ChallengesPerDungeon);

            if (!announcedCompletion && IsComplete)
            {
                announcedCompletion = true;
                DungeonCompleted?.Invoke(dungeonId);
            }
        }

        private void OnChallengeSolved(int challengeId)
        {
            // Challenge ids are dungeon-scoped, so ignore anything not ours.
            if (challengeId / 100 != dungeonId)
            {
                return;
            }

            int solved = SolvedCount;
            ProgressChanged?.Invoke(solved, GameConstants.ChallengesPerDungeon);

            if (solved < GameConstants.ChallengesPerDungeon || announcedCompletion)
            {
                return;
            }

            announcedCompletion = true;

            // Marks completion, unlocks the next dungeon and auto-saves (M2).
            GameManager.Instance?.NotifyDungeonCompleted(dungeonId);
            Debug.Log($"[DungeonManager] Dungeon {dungeonId} complete; dungeon {dungeonId + 1} unlocked if it exists.", this);
            DungeonCompleted?.Invoke(dungeonId);
        }

        private int CountSolvedHere(DungeonProgress progress)
        {
            List<int> solved = progress.challengesSolved;
            int count = 0;
            for (int i = 0; i < solved.Count; i++)
            {
                if (solved[i] / 100 == dungeonId)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
