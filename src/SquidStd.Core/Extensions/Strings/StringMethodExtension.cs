using SquidStd.Core.Utils;

namespace SquidStd.Core.Extensions.Strings;

/// <summary>
///     Provides extension methods for string operations, particularly for case conversions.
/// </summary>
public static class StringMethodExtension
{
    /// <param name="text">The string to convert.</param>
    extension(string text)
    {
        /// <summary>
        ///     Converts a string to camelCase.
        /// </summary>
        /// <returns>A camelCase version of the input string.</returns>
        public string ToCamelCase()
        {
            return StringUtils.ToCamelCase(text);
        }

        /// <summary>
        ///     Converts a string to Dot Case.
        /// </summary>
        /// <returns>A Dot Case version of the input string.</returns>
        public string ToDotCase()
        {
            return StringUtils.ToDotCase(text);
        }

        /// <summary>
        ///     Converts a string to kebab-case.
        /// </summary>
        /// <returns>A kebab-case version of the input string.</returns>
        public string ToKebabCase()
        {
            return StringUtils.ToKebabCase(text);
        }

        /// <summary>
        ///     Converts a string to PascalCase.
        /// </summary>
        /// <returns>A PascalCase version of the input string.</returns>
        public string ToPascalCase()
        {
            return StringUtils.ToPascalCase(text);
        }

        /// <summary>
        ///     Converts a string to Path Case.
        /// </summary>
        /// <returns>A Path Case version of the input string.</returns>
        public string ToPathCase()
        {
            return StringUtils.ToPathCase(text);
        }

        /// <summary>
        ///     Converts a string to Sentence Case.
        /// </summary>
        /// <returns>A Sentence Case version of the input string.</returns>
        public string ToSentenceCase()
        {
            return StringUtils.ToSentenceCase(text);
        }

        /// <summary>
        ///     Converts a string to snake_case.
        /// </summary>
        /// <returns>A snake_case version of the input string.</returns>
        public string ToSnakeCase()
        {
            return StringUtils.ToSnakeCase(text);
        }

        /// <summary>
        ///     Converts a string to UPPER_SNAKE_CASE.
        /// </summary>
        /// <returns>An UPPER_SNAKE_CASE version of the input string.</returns>
        public string ToSnakeCaseUpper()
        {
            return StringUtils.ToUpperSnakeCase(text);
        }

        /// <summary>
        ///     Converts a string to Title Case.
        /// </summary>
        /// <returns>A Title Case version of the input string.</returns>
        public string ToTitleCase()
        {
            return StringUtils.ToTitleCase(text);
        }

        /// <summary>
        ///     Converts a string to Train Case.
        /// </summary>
        /// <returns>A Train Case version of the input string.</returns>
        public string ToTrainCase()
        {
            return StringUtils.ToTrainCase(text);
        }
    }
}
