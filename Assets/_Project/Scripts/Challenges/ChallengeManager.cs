using System;
using MathDungeon.Core;
using MathDungeon.Data;
using UnityEngine;

namespace MathDungeon.Challenges
{
    /// <summary>
    /// The mathematics subsystem (SRS 1.5): picks the predefined challenge for a
    /// terminal, validates the answer, and drives hint and explanation feedback.
    /// It owns no question text and draws no UI - it sits between
    /// <see cref="ChallengeLibrary"/> and <see cref="ChallengeUI"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChallengeManager : MonoBehaviour
    {
        [SerializeField] private ChallengeLibrary library;
        [SerializeField] private ChallengeUI challengeUI;

        private ChallengeData activeChallenge;
        private bool awaitingContinue;

        /// <summary>Raised with the challengeId once an answer is accepted.</summary>
        public event Action<int> ChallengeSolved;

        /// <summary>Raised with the challengeId and the running attempt count after a wrong answer.</summary>
        public event Action<int, int> ChallengeAttemptFailed;

        public bool IsChallengeOpen => activeChallenge != null;

        private void OnEnable()
        {
            if (challengeUI != null)
            {
                challengeUI.AnswerSubmitted += OnAnswerSubmitted;
                challengeUI.ContinueRequested += CloseChallenge;
            }
        }

        private void OnDisable()
        {
            if (challengeUI != null)
            {
                challengeUI.AnswerSubmitted -= OnAnswerSubmitted;
                challengeUI.ContinueRequested -= CloseChallenge;
            }
        }

        /// <summary>
        /// Opens the challenge a terminal owns. Wired to
        /// <c>ChallengeTerminal.activated</c>, which passes its challengeId.
        /// </summary>
        public void BeginChallenge(int challengeId)
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

            // Freezing the player is the state's job, not this class's (see M3
            // GameStateExtensions), so it only needs to announce the state change.
            GameManager.Instance?.SetState(GameState.InChallenge);

            if (challengeUI != null)
            {
                challengeUI.Show(data.Challenge);
            }
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
            // The attempt count drives the hint step-up in FR7, so it is tracked
            // even though hearts are the health system's concern (M5).
            int attempts = RegisterAttempt(challenge);

            if (challengeUI != null)
            {
                challengeUI.ShowHint(challenge.hintText);
            }

            ChallengeAttemptFailed?.Invoke(challenge.challengeId, attempts);
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
            activeChallenge = null;
            awaitingContinue = false;

            if (challengeUI != null)
            {
                challengeUI.Hide();
            }

            GameManager.Instance?.SetState(GameState.Exploring);
        }
    }
}
