using System;
using System.Collections.Generic;
using MathDungeon.Core;
using MathDungeon.Data;
using MathDungeon.Player;
using UnityEngine;

namespace MathDungeon.Challenges
{
    /// <summary>
    /// The mathematics subsystem (SRS 1.5): picks the predefined challenge for a
    /// terminal or an enemy encounter, validates the answer, and drives hint and
    /// explanation feedback. It owns no question text and draws no UI - it sits
    /// between <see cref="ChallengeLibrary"/> and <see cref="ChallengeUI"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChallengeManager : MonoBehaviour
    {
        [SerializeField] private ChallengeLibrary library;
        [SerializeField] private ChallengeUI challengeUI;

        [Tooltip("Hearts are deducted here on a wrong answer. Found in the scene if left empty.")]
        [SerializeField] private PlayerHealth playerHealth;

        private ChallengeData activeChallenge;
        private bool awaitingContinue;

        /// <summary>Raised with the challengeId once an answer is accepted.</summary>
        public event Action<int> ChallengeSolved;

        /// <summary>Raised with the challengeId and the running attempt count after a wrong answer.</summary>
        public event Action<int, int> ChallengeAttemptFailed;

        /// <summary>Raised when the panel closes, saying how it started and whether it was solved.</summary>
        public event Action<ChallengeSource, bool> ChallengeClosed;

        public bool IsChallengeOpen => activeChallenge != null;

        /// <summary>What opened the current (or most recent) challenge.</summary>
        public ChallengeSource CurrentSource { get; private set; }

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }
        }

        private void OnEnable()
        {
            if (challengeUI != null)
            {
                challengeUI.AnswerSubmitted += OnAnswerSubmitted;
                challengeUI.ContinueRequested += CloseChallenge;
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (challengeUI != null)
            {
                challengeUI.AnswerSubmitted -= OnAnswerSubmitted;
                challengeUI.ContinueRequested -= CloseChallenge;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged -= OnGameStateChanged;
            }
        }

        /// <summary>
        /// Something outside the challenge changed the game state - Game Over, or
        /// the run finishing. Close the panel so it cannot sit on top of whatever
        /// screen that state owns.
        /// </summary>
        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.InChallenge && IsChallengeOpen)
            {
                EndChallenge(false, restoreExploring: false);
            }
        }

        /// <summary>
        /// Opens the challenge a terminal owns. Wired to
        /// <c>ChallengeTerminal.activated</c>, which passes its challengeId.
        /// </summary>
        public void BeginChallenge(int challengeId)
        {
            BeginChallenge(challengeId, ChallengeSource.Terminal);
        }

        /// <summary>
        /// Opens an unsolved challenge from the given dungeon because an enemy
        /// caught the player. Question selection is this subsystem's job per the
        /// SRS architecture, so the enemy does not choose the maths.
        /// </summary>
        public bool BeginEnemyEncounter(int dungeonId)
        {
            ChallengeData chosen = SelectEncounterChallenge(dungeonId);
            if (chosen == null)
            {
                Debug.LogWarning($"[ChallengeManager] No challenge available for an encounter in dungeon {dungeonId}.", this);
                return false;
            }

            BeginChallenge(chosen.ChallengeId, ChallengeSource.Enemy);
            return IsChallengeOpen;
        }

        private void BeginChallenge(int challengeId, ChallengeSource source)
        {
            if (IsChallengeOpen)
            {
                return;
            }

            if (library == null)
            {
                Debug.LogError("[ChallengeManager] No challenge library assigned.", this);
                return;
            }

            if (!library.TryGet(challengeId, out ChallengeData data))
            {
                Debug.LogError($"[ChallengeManager] No challenge authored with id {challengeId}.", this);
                return;
            }

            activeChallenge = data;
            awaitingContinue = false;
            CurrentSource = source;

            // Freezing the player is the state's job, not this class's (see M3
            // GameStateExtensions), so it only needs to announce the state change.
            GameManager.Instance?.SetState(GameState.InChallenge);

            if (challengeUI != null)
            {
                challengeUI.Show(data.Challenge);
            }
        }

        /// <summary>
        /// Prefers a challenge the player has not solved yet, so an encounter
        /// teaches something new; falls back to any challenge in the dungeon once
        /// they are all solved, which keeps the enemy a threat on a replay.
        /// </summary>
        private ChallengeData SelectEncounterChallenge(int dungeonId)
        {
            List<ChallengeData> candidates = library != null ? library.GetForDungeon(dungeonId) : null;
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            DungeonProgress progress = GameManager.Instance != null && GameManager.Instance.HasActiveProfile
                ? GameManager.Instance.ActiveProfile.GetProgress(dungeonId)
                : null;

            if (progress != null)
            {
                foreach (ChallengeData candidate in candidates)
                {
                    if (!progress.IsChallengeSolved(candidate.ChallengeId))
                    {
                        return candidate;
                    }
                }
            }

            return candidates[0];
        }

        private void OnAnswerSubmitted(string answer)
        {
            if (!IsChallengeOpen || awaitingContinue)
            {
                return;
            }

            Challenge challenge = activeChallenge.Challenge;

            if (AnswerValidator.IsCorrect(challenge, answer))
            {
                HandleCorrectAnswer(challenge);
            }
            else
            {
                HandleIncorrectAnswer(challenge);
            }
        }

        private void HandleCorrectAnswer(Challenge challenge)
        {
            awaitingContinue = true;

            // Recording progress and auto-saving belongs to the GameManager (M2).
            GameManager.Instance?.NotifyChallengeSolved(challenge.dungeonId, challenge.challengeId);

            if (challengeUI != null)
            {
                challengeUI.ShowExplanation(challenge.explanationText);
            }

            ChallengeSolved?.Invoke(challenge.challengeId);
        }

        private void HandleIncorrectAnswer(Challenge challenge)
        {
            // The attempt count drives the hint step-up in FR7.
            int attempts = RegisterAttempt(challenge);
            ChallengeAttemptFailed?.Invoke(challenge.challengeId, attempts);

            // Every wrong challenge answer costs a heart, whichever route opened it
            // (SRS FR6 Health System).
            bool stillAlive = playerHealth == null || playerHealth.LoseHeart();

            if (!stillAlive)
            {
                // PlayerHealth has already moved the game into the Game Over state;
                // close the panel so the Game Over choice is what the player sees.
                EndChallenge(false, restoreExploring: false);
                return;
            }

            if (challengeUI != null)
            {
                challengeUI.ShowHint(challenge.hintText);
            }
        }

        private int RegisterAttempt(Challenge challenge)
        {
            PlayerProfile profile = GameManager.Instance != null ? GameManager.Instance.ActiveProfile : null;
            DungeonProgress progress = profile?.GetProgress(challenge.dungeonId);
            return progress != null ? progress.RegisterIncorrectAttempt(challenge.challengeId) : 0;
        }

        /// <summary>Closes the panel and hands control back to exploration.</summary>
        public void CloseChallenge()
        {
            bool solved = awaitingContinue;
            EndChallenge(solved, restoreExploring: true);
        }

        private void EndChallenge(bool solved, bool restoreExploring)
        {
            ChallengeSource source = CurrentSource;
            activeChallenge = null;
            awaitingContinue = false;

            if (challengeUI != null)
            {
                challengeUI.Hide();
            }

            if (restoreExploring)
            {
                GameManager.Instance?.SetState(GameState.Exploring);
            }

            ChallengeClosed?.Invoke(source, solved);
        }
    }
}
