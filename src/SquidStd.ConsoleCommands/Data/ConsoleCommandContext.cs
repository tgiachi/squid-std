namespace SquidStd.ConsoleCommands.Data;

/// <summary>
/// Execution context handed to a console command handler.
/// </summary>
public sealed class ConsoleCommandContext
{
    private readonly Action<string> _writeLine;

    /// <summary>The raw command line as typed.</summary>
    public string RawText { get; }

    /// <summary>Parsed arguments (command name excluded).</summary>
    public IReadOnlyList<string> Arguments { get; }

    /// <summary>Initializes the context.</summary>
    /// <param name="rawText">Raw command line.</param>
    /// <param name="arguments">Parsed arguments.</param>
    /// <param name="writeLine">Writer for command output lines.</param>
    public ConsoleCommandContext(string rawText, IReadOnlyList<string> arguments, Action<string> writeLine)
    {
        ArgumentNullException.ThrowIfNull(writeLine);

        RawText = rawText;
        Arguments = arguments;
        _writeLine = writeLine;
    }

    /// <summary>Writes one output line.</summary>
    public void WriteLine(string line)
        => _writeLine(line);
}
