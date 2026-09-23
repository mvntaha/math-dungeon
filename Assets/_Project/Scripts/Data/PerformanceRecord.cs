using System;

namespace MathDungeon.Data
{
    /// <summary>
    /// Summary of a single play session (SRS 1.9 PerformanceRecord), used by the
    /// performance summary screen and the cumulative learning progress report.
    /// </summary>
    [Serializable]
    public class PerformanceRecord
    {
        public string sessionId;
        public string profileId;
        public int correctAnswers;
        public int incorrectAttempts;

        /// <summary>correctAnswers / totalAttempts x 100.</summary>
        public float accuracyPercent;

        public int coinsEarned;
        public int totalScore;

        /// <summary>Session end timestamp, ISO 8601.</summary>
        public string completedAt;

        public PerformanceRecord() { }

        /// <summary>
        /// Recalculates <see cref="accuracyPercent"/> from the current answer counts.
        /// Returns 0 when no attempts have been made.
        /// </summary>
        public void RecalculateAccuracy()
        {
            int totalAttempts = correctAnswers + incorrectAttempts;
            accuracyPercent = totalAttempts == 0
                ? 0f
                : (float)correctAnswers / totalAttempts * 100f;
        }
    }
}
