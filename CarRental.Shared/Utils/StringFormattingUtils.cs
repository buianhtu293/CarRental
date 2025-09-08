using System.Text.RegularExpressions;

namespace CarRental.Shared.Utils
{
    /// <summary>
    /// Provides utility methods for manipulating and formatting strings.
    /// </summary>
    public static partial class StringFormattingUtils
    {
        // Compiled regex for performance in repeated calls.
        // Pattern explanation:
        //   (?<=[a-z])(?=[A-Z]) → Position between a lowercase and uppercase letter
        //   | (?<=[A-Z])(?=[A-Z][a-z]) → Position between uppercase sequences and a capitalized word
        [GeneratedRegex(@"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled)]
        private static partial Regex CamelCaseBoundaryRegex();

        /// <summary>
        /// Inserts spaces between words in a camelCase or PascalCase string.
        /// </summary>
        /// <param name="input">The camelCase or PascalCase string to process.</param>
        /// <returns>
        /// A new string with spaces inserted between the detected word boundaries.
        /// Returns the original string if no camel case pattern is detected.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
        /// <example>
        /// <code>
        /// var result = StringFormattingUtils.InsertSpacesInCamelCase("ThisIsATest");
        /// // result = "This Is A Test"
        /// </code>
        /// </example>
        public static string InsertSpacesInCamelCase(string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length == 0)
            {
                return string.Empty;
            }

            return CamelCaseBoundaryRegex().Replace(input, " ");
        }
    }
}