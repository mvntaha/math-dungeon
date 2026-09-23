using System.Collections.Generic;
using MathDungeon.Core;
using UnityEngine;

namespace MathDungeon.Challenges
{
    /// <summary>
    /// The complete authored challenge set: 3 dungeons x 4 challenges (SRS 1.3).
    /// The challenge manager looks questions up here by id, so scenes never hold
    /// question content themselves.
    /// </summary>
    [CreateAssetMenu(fileName = "ChallengeLibrary", menuName = "Math Dungeon/Challenge Library")]
    public class ChallengeLibrary : ScriptableObject
    {
        [SerializeField] private List<ChallengeData> challenges = new List<ChallengeData>();

        public IReadOnlyList<ChallengeData> All => challenges;

        public bool TryGet(int challengeId, out ChallengeData data)
        {
            for (int i = 0; i < challenges.Count; i++)
            {
                if (challenges[i] != null && challenges[i].ChallengeId == challengeId)
                {
                    data = challenges[i];
                    return true;
                }
            }

            data = null;
            return false;
        }

        public List<ChallengeData> GetForDungeon(int dungeonId)
        {
            List<ChallengeData> result = new List<ChallengeData>();
            for (int i = 0; i < challenges.Count; i++)
            {
                if (challenges[i] != null && challenges[i].DungeonId == dungeonId)
                {
                    result.Add(challenges[i]);
                }
            }

            return result;
        }

        private void OnValidate()
        {
            if (challenges.Count != GameConstants.TotalChallenges)
            {
                Debug.LogWarning(
                    $"[{name}] holds {challenges.Count} challenges; the SRS specifies {GameConstants.TotalChallenges}.", this);
            }

            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < challenges.Count; i++)
            {
                if (challenges[i] != null && !seen.Add(challenges[i].ChallengeId))
                {
                    Debug.LogWarning($"[{name}] has a duplicate challengeId: {challenges[i].ChallengeId}.", this);
                }
            }
        }
    }
}
