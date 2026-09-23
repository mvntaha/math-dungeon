namespace MathDungeon.Data
{
    /// <summary>
    /// The three question formats defined by SRS 1.9 (Challenge.questionType).
    /// No other formats exist in this project.
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice = 0,
        NumericalInput = 1,
        PatternMatch = 2
    }

    /// <summary>
    /// Converts between <see cref="QuestionType"/> and the snake_case strings
    /// used in the SRS data dictionary and in the authored challenge JSON.
    /// </summary>
    public static class QuestionTypeExtensions
    {
        public const string MultipleChoiceKey = "multiple_choice";
        public const string NumericalInputKey = "numerical_input";
        public const string PatternMatchKey = "pattern_match";

        public static string ToSerializedKey(this QuestionType questionType)
        {
            switch (questionType)
            {
                case QuestionType.MultipleChoice: return MultipleChoiceKey;
                case QuestionType.NumericalInput: return NumericalInputKey;
                case QuestionType.PatternMatch: return PatternMatchKey;
                default: return MultipleChoiceKey;
            }
        }

        public static bool TryParse(string key, out QuestionType questionType)
        {
            switch (key)
            {
                case MultipleChoiceKey:
                    questionType = QuestionType.MultipleChoice;
                    return true;
                case NumericalInputKey:
                    questionType = QuestionType.NumericalInput;
                    return true;
                case PatternMatchKey:
                    questionType = QuestionType.PatternMatch;
                    return true;
                default:
                    questionType = QuestionType.MultipleChoice;
                    return false;
            }
        }
    }
}
