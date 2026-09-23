using MathDungeon.Core;
using UnityEngine;

namespace MathDungeon.Dungeon
{
    /// <summary>
    /// The barred exit of a dungeon (SRS 1.2: a correct solution unlocks a gate).
    /// It stays shut until every challenge in the dungeon is solved, then opens and
    /// leads to the next dungeon - or, after the last one, to the completion state.
    /// </summary>
    [DisallowMultipleComponent]
    public class GateController : MonoBehaviour
    {
        [Tooltip("Found in the scene when left empty.")]
        [SerializeField] private DungeonManager dungeonManager;

        [Tooltip("The bars themselves. Hidden once the gate opens.")]
        [SerializeField] private GameObject lockedBarrier;

        [Tooltip("Blocks the player while the gate is shut.")]
        [SerializeField] private Collider blockingCollider;

        [Tooltip("Entering this while the gate is open moves to the next dungeon.")]
        [SerializeField] private Collider passageTrigger;

        private bool isOpen;
        private bool transitioning;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (dungeonManager == null)
            {
                dungeonManager = FindFirstObjectByType<DungeonManager>();
            }

            SetOpen(false);
        }

        private void OnEnable()
        {
            if (dungeonManager != null)
            {
                dungeonManager.DungeonCompleted += OnDungeonCompleted;
            }
        }

        private void OnDisable()
        {
            if (dungeonManager != null)
            {
                dungeonManager.DungeonCompleted -= OnDungeonCompleted;
            }
        }

        private void OnDungeonCompleted(int dungeonId)
        {
            SetOpen(true);
            Debug.Log($"[GateController] Dungeon {dungeonId} gate opened.", this);
        }

        private void SetOpen(bool open)
        {
            isOpen = open;

            if (lockedBarrier != null)
            {
                lockedBarrier.SetActive(!open);
            }

            if (blockingCollider != null)
            {
                blockingCollider.enabled = !open;
            }

            if (passageTrigger != null)
            {
                passageTrigger.enabled = open;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isOpen || transitioning || !other.CompareTag("Player"))
            {
                return;
            }

            transitioning = true;
            Advance();
        }

        /// <summary>
        /// Moves on to the next dungeon, or finishes the run after the last one.
        /// Also callable from the Inspector for testing without walking through.
        /// </summary>
        public void Advance()
        {
            // A shut gate leads nowhere, however it was called from.
            if (!isOpen)
            {
                transitioning = false;
                return;
            }

            GameManager manager = GameManager.Instance;
            if (manager == null || dungeonManager == null)
            {
                return;
            }

            // Never change dungeon out from under an open challenge.
            Challenges.ChallengeManager challenges = FindFirstObjectByType<Challenges.ChallengeManager>();
            if (challenges != null && challenges.IsChallengeOpen)
            {
                transitioning = false;
                return;
            }

            int nextDungeon = dungeonManager.DungeonId + 1;

            if (!GameConstants.IsValidDungeonId(nextDungeon))
            {
                // Past the last dungeon: the run is over. The full performance
                // summary belongs to M7; this just raises the state for it.
                manager.SetState(GameState.DungeonComplete);
                transitioning = false;
                return;
            }

            if (!manager.HasActiveProfile || !manager.ActiveProfile.IsDungeonUnlocked(nextDungeon))
            {
                Debug.LogWarning($"[GateController] Dungeon {nextDungeon} is not unlocked; staying put.", this);
                transitioning = false;
                return;
            }

            manager.LoadDungeon(nextDungeon);
        }
    }
}
