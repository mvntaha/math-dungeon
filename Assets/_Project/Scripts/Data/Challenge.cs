using System;
using UnityEngine;

namespace MathDungeon.Data
{
    /// <summary>
    /// One predefined mathematics challenge (SRS 1.9 Challenge).
    /// Read-only authored content - never generated at runtime.
    /// </summary>
    [Serializable]
    public class Challenge
    {
        public int challengeId;
        public int dungeonId;
        public string topic;
        public QuestionType questionType;
        [TextArea] public string promptText;

        /// <summary>Answer choices. Used only when <see cref="questionType"/> is MultipleChoice; null/empty otherwise.</summary>
        public string[] options;

        public string correctAnswer;
        [TextArea] public string hintText;
        [TextArea] public string explanationText;

        public bool HasOptions => options != null && options.Length > 0;
    }
}
