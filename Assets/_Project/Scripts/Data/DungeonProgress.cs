using System;
using System.Collections.Generic;
using MathDungeon.Core;

namespace MathDungeon.Data
{
    /// <summary>
    /// Per-dungeon completion state (SRS 1.9 DungeonProgress), nested in <see cref="PlayerProfile"/>.
    /// </summary>
    [Serializable]
    public class DungeonProgress
    {
        public int dungeonId;
        public bool completed;
        public List<int> challengesSolved = new List<int>();

        /// <summary>
        /// Incorrect-attempt count per challenge, keyed by challengeId, used for the
        /// hint step-up in FR7. Serializes as the SRS-specified object, e.g. {"203": 2}.
        /// </summary>
        public Dictionary<int, int> attemptsPerChallenge = new Dictionary<int, int>();

        public int elapsedTimeSeconds;

        public DungeonProgress() { }

        public DungeonProgress(int dungeonId)
        {
            this.dungeonId = dungeonId;
        }

        public bool IsChallengeSolved(int challengeId)
        {
            return challengesSolved.Contains(challengeId);
        }

        /// <summary>Records a solved challenge and refreshes <see cref="completed"/>.</summary>
        public void MarkChallengeSolved(int challengeId)
        {
            if (!challengesSolved.Contains(challengeId))
            {
                challengesSolved.Add(challengeId);
            }

            completed = challengesSolved.Count >= GameConstants.ChallengesPerDungeon;
        }

        public int GetAttempts(int challengeId)
        {
            int attempts;
            return attemptsPerChallenge.TryGetValue(challengeId, out attempts) ? attempts : 0;
        }

        /// <summary>Increments and returns the incorrect-attempt count for a challenge.</summary>
        public int RegisterIncorrectAttempt(int challengeId)
        {
            int attempts = GetAttempts(challengeId) + 1;
            attemptsPerChallenge[challengeId] = attempts;
            return attempts;
        }
    }
}
