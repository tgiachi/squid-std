using System.Collections;
using System.Text.RegularExpressions;

namespace SquidStd.Core.Extensions.Env;

/// <summary>
/// Provides extension methods for expanding environment variables in strings
/// </summary>
public static class EnvExtensions
{
    private static readonly Regex EnvTokenRegex = new(
        @"\$([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled
    );

    /// <param name="input">The input string.</param>
    extension(string input)
    {
        /// <summary>
        /// Expands environment variables in a string using custom $VARIABLE syntax
        /// </summary>
        /// <returns>The string with environment variables expanded to their values</returns>
        public string ExpandEnvironmentVariables()
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                var key = $"${env.Key}";
                var value = env.Value?.ToString() ?? string.Empty;
                input = input.Replace(key, value);
            }

            return input;
        }

        /// <summary>
        /// Replaces "$VAR" tokens with the matching environment variable value. Unknown variables are
        /// left unchanged.
        /// </summary>
        /// <returns>The string with known $VAR tokens substituted.</returns>
        public string ReplaceEnv()
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return EnvTokenRegex.Replace(
                input,
                match =>
                {
                    var name = match.Groups[1].Value;
                    var value = Environment.GetEnvironmentVariable(name);

                    return value ?? match.Value;
                }
            );
        }
    }
}
