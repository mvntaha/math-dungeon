using System.Globalization;
using MathDungeon.Data;

namespace MathDungeon.Challenges
{
    /// <summary>
    /// Decides whether a submitted answer is correct (SRS FR4 Answer Validation).
    /// Kept separate from the challenge manager so the rules for each question type
    /// can be read and changed in one place.
    /// </summary>
    public static class AnswerValidator
    {
        private const double NumericTolerance = 1e-6;

        public static bool IsCorrect(Challenge challenge, string submitted)
        {
            if (challenge == null || submitted == null)
            {
                return false;
            }

            switch (challenge.questionType)
            {
                case QuestionType.MultipleChoice:
                    // The player picks an option, so an exact match to the stored
                    // answer is what counts - no numeric interpretation.
                    return TextMatches(submitted, challenge.correctAnswer);

                case QuestionType.NumericalInput:
                    return NumbersMatch(submitted, challenge.correctAnswer);

                case QuestionType.PatternMatch:
                    // Pattern answers are usually the next term, but may be a short
                    // sequence, so compare numerically first and fall back to text.
                    return NumbersMatch(submitted, challenge.correctAnswer)
                           || SequenceMatches(submitted, challenge.correctAnswer);

                default:
                    return false;
            }
        }

        private static bool TextMatches(string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return string.Equals(a.Trim(), b.Trim(), System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares two answers as numbers, so "3", "3.0" and " 3 " all count.
        /// Returns false when either side is not a number.
        /// </summary>
        private static bool NumbersMatch(string submitted, string expected)
        {
            return TryParseNumber(submitted, out double left)
                   && TryParseNumber(expected, out double right)
                   && System.Math.Abs(left - right) <= NumericTolerance;
        }

        private static bool TryParseNumber(string value, out double result)
        {
            result = 0d;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return double.TryParse(
                value.Trim(),
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out result);
        }

        /// <summary>
        /// Compares comma or space separated sequences, ignoring spacing and case,
        /// so "2, 4, 8" and "2 4 8" are the same answer.
        /// </summary>
        private static bool SequenceMatches(string submitted, string expected)
        {
            return string.Equals(Normalize(submitted), Normalize(expected), System.StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length);
            bool lastWasSeparator = false;

            foreach (char c in value.Trim())
            {
                if (char.IsWhiteSpace(c) || c == ',' || c == ';')
                {
                    if (builder.Length > 0 && !lastWasSeparator)
                    {
                        builder.Append(',');
                        lastWasSeparator = true;
                    }

                    continue;
                }

                builder.Append(char.ToLowerInvariant(c));
                lastWasSeparator = false;
            }

            if (builder.Length > 0 && builder[builder.Length - 1] == ',')
            {
                builder.Length--;
            }

            return builder.ToString();
        }
    }
}
