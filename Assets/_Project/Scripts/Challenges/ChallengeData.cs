using MathDungeon.Data;
using UnityEngine;

namespace MathDungeon.Challenges
{
    /// <summary>
    /// One authored mathematics challenge as a project asset. All 12 challenges are
    /// hand-written content (SRS 1.3) - nothing is generated at runtime, and no
    /// question text is ever hard-coded inside a MonoBehaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "Challenge_", menuName = "Math Dungeon/Challenge")]
    public class ChallengeData : ScriptableObject
    {
        [SerializeField] private Challenge challenge = new Challenge();

        public Challenge Challenge => challenge;

        public int ChallengeId => challenge.challengeId;

        public int DungeonId => challenge.dungeonId;

        public QuestionType QuestionType => challenge.questionType;

        private void OnValidate()
        {
            if (challenge.questionType == QuestionType.MultipleChoice && !challenge.HasOptions)
            {
                Debug.LogWarning($"[{name}] is multiple_choice but has no options.", this);
            }

            if (challenge.questionType != QuestionType.MultipleChoice && challenge.HasOptions)
            {
                Debug.LogWarning($"[{name}] is {challenge.questionType} but still carries options; they will be ignored.", this);
            }
        }
    }
}
