using System;
using MathDungeon.Challenges;
using MathDungeon.Data;
using UnityEngine;

namespace MathDungeon.Core
{
    /// <summary>
    /// Builds the <see cref="PerformanceRecord"/> for the current play session
    /// (SRS FR9 Scoring and Performance Evaluation) by counting answers as they
    /// happen. It survives scene changes alongside the GameManager, so a record
    /// spans the whole run rather than a single dungeon.
    ///
    /// Note the deliberate gap: coinsEarned and totalScore are the reward system's
    /// to fill in (M8). This class records what the challenge flow already knows.
    /// </summary>
    [DisallowMultipleComponent]
    public class SessionTracker : MonoBehaviour
    {
        public static SessionTracker Instance { get; private set; }

        private ChallengeManager subscribedManager;
        private float sessionStartTime;

        /// <summary>The running record for this session.</summary>
        public PerformanceRecord Record { get; private set; } = new PerformanceRecord();

        /// <summary>Seconds since the session began, for the HUD and the summary.</summary>
        public float ElapsedSeconds => Time.time - sessionStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BeginSession();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Starts a fresh record. Called when a new game or a continue begins.</summary>
        public void BeginSession()
        {
            sessionStartTime = Time.time;
            Record = new PerformanceRecord
            {
                sessionId = Guid.NewGuid().ToString("N").Substring(0, 8),
                profileId = GameManager.Instance != null && GameManager.Instance.HasActiveProfile
                    ? GameManager.Instance.ActiveProfile.profileId
                    : string.Empty
            };
        }

        private void Update()
        {
            // The challenge manager lives per scene, so re-attach after a load.
            ChallengeManager manager = ChallengeManagerInScene();
            if (manager != subscribedManager)
            {
                Unsubscribe();
                subscribedManager = manager;
                if (subscribedManager != null)
                {
                    subscribedManager.ChallengeSolved += OnChallengeSolved;
                    subscribedManager.ChallengeAttemptFailed += OnChallengeAttemptFailed;
                }
            }
        }

        private static ChallengeManager ChallengeManagerInScene()
        {
            return FindFirstObjectByType<ChallengeManager>();
        }

        private void Unsubscribe()
        {
            if (subscribedManager == null)
            {
                return;
            }

            subscribedManager.ChallengeSolved -= OnChallengeSolved;
            subscribedManager.ChallengeAttemptFailed -= OnChallengeAttemptFailed;
            subscribedManager = null;
        }

        private void OnChallengeSolved(int challengeId)
        {
            Record.correctAnswers++;
            Refresh();
        }

        private void OnChallengeAttemptFailed(int challengeId, int attempts)
        {
            Record.incorrectAttempts++;
            Refresh();
        }

        private void Refresh()
        {
            Record.RecalculateAccuracy();

            if (GameManager.Instance != null && GameManager.Instance.HasActiveProfile)
            {
                Record.profileId = GameManager.Instance.ActiveProfile.profileId;
                Record.coinsEarned = GameManager.Instance.ActiveProfile.coins;
            }
        }

        /// <summary>Stamps the record as finished, for the performance summary.</summary>
        public PerformanceRecord CompleteSession()
        {
            Refresh();
            Record.completedAt = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return Record;
        }
    }
}
