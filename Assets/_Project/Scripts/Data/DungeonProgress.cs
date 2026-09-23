using System;
using System.Collections.Generic;
using MathDungeon.Core;

namespace MathDungeon.Data
{
    /// <summary>
    /// Incorrect-attempt count for a single challenge.
    /// The SRS models attemptsPerChallenge as an object keyed by challengeId;
    /// Unity's JsonUtility cannot serialize dictionaries, so it is stored as a
    /// list of entries and accessed through the helpers on <see cref="DungeonProgress"/>.
    /// </summary>
    [Serializable]
    public class ChallengeAttemptEntry
    {
        public int challengeId;
        public int attempts;

        public ChallengeAttemptEntry() { }

        public ChallengeAttemptEntry(int challengeId, int attempts)
        {
            this.challengeId = challengeId;
            this.attempts = attempts;
        }
    }

    /// <summary>
    /// Per-dungeon completion state (SRS 1.9 DungeonProgress), nested in <see cref="PlayerProfile"/>.
    /// </summary>
    [Serializable]
    public class DungeonProgress
    {
        public int dungeonId;
        public bool completed;
        public List<int> challengesSolved = new List<int>();
        public List<ChallengeAttemptEntry> attemptsPerChallenge = new List<ChallengeAttemptEntry>();
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
            ChallengeAttemptEntry entry = FindEntry(challengeId);
            return entry == null ? 0 : entry.attempts;
        }

        /// <summary>Increments and returns the incorrect-attempt count for a challenge.</summary>
        public int RegisterIncorrectAttempt(int challengeId)
        {
            ChallengeAttemptEntry entry = FindEntry(challengeId);
            if (entry == null)
            {
                entry = new ChallengeAttemptEntry(challengeId, 0);
                attemptsPerChallenge.Add(entry);
            }

            entry.attempts++;
            return entry.attempts;
        }

        private ChallengeAttemptEntry FindEntry(int challengeId)
        {
            for (int i = 0; i < attemptsPerChallenge.Count; i++)
            {
                if (attemptsPerChallenge[i].challengeId == challengeId)
                {
                    return attemptsPerChallenge[i];
                }
            }

            return null;
        }
    }
}
