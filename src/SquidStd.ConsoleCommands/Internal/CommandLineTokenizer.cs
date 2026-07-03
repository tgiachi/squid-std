namespace SquidStd.ConsoleCommands.Internal;

/// <summary>
/// Splits a raw command line into tokens, honoring double-quoted segments.
/// </summary>
internal static class CommandLineTokenizer
{
    internal static IReadOnlyList<string> Tokenize(string input)
    {
        var tokens = new List<string>();

        if (string.IsNullOrWhiteSpace(input))
        {
            return tokens;
        }

        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        var hasToken = false;

        foreach (var character in input)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                hasToken = true;

                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                if (hasToken)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                    hasToken = false;
                }

                continue;
            }

            current.Append(character);
            hasToken = true;
        }

        if (hasToken)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
