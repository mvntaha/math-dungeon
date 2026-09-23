using MathDungeon.Challenges;
using MathDungeon.Player;
using UnityEngine;
using UnityEngine.Events;

namespace MathDungeon.Dungeon
{
    /// <summary>
    /// A terminal in the world that the player activates to open its mathematics
    /// challenge (SRS FR4 Problem Selection, FR5 Object Interaction).
    ///
    /// Milestone 3 provides the interaction half only: it identifies which challenge
    /// it owns and raises an event. Milestone 4 hands that challenge id to the
    /// ChallengeManager, which is what actually presents the question.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChallengeTerminal : MonoBehaviour, IInteractable
    {
        [Header("Challenge")]
        [Tooltip("Id of the Challenge asset this terminal presents (SRS 1.9).")]
        [SerializeField] private int challengeId;

        [SerializeField] private string prompt = "Examine terminal";

        [Header("State")]
        [Tooltip("Solved terminals stay in the world but can no longer be activated.")]
        [SerializeField] private bool solved;

        [Header("Events")]
        [Tooltip("Raised when the player activates this terminal. M4 listens here.")]
        [SerializeField] private UnityEvent<int> activated;

        private ChallengeManager challengeManager;

        public int ChallengeId => challengeId;

        public string InteractionPrompt => prompt;

        public bool CanInteract => !solved;

        private void OnEnable()
        {
            // Listen for this terminal's own challenge being solved so it stops
            // offering it, including when the manager is reached via its UnityEvent.
            challengeManager = FindFirstObjectByType<ChallengeManager>();
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
        }

        private void OnChallengeSolved(int solvedChallengeId)
        {
            if (solvedChallengeId == challengeId)
            {
                MarkSolved();
            }
        }

        public void Interact(PlayerInteraction interactor)
        {
            if (!CanInteract)
            {
                return;
            }

            Debug.Log($"[ChallengeTerminal] Activated terminal for challenge {challengeId}.", this);
            activated?.Invoke(challengeId);
        }

        /// <summary>Marks the terminal solved so it stops offering its challenge.</summary>
        public void MarkSolved()
        {
            solved = true;
        }
    }
}
