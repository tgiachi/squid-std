using Serilog.Events;

namespace SquidStd.ConsoleCommands.Interfaces;

/// <summary>
/// Renders the command prompt and coordinates command output and log lines on the terminal.
/// </summary>
public interface IConsoleUiService
{
    /// <summary>Whether the console is an interactive terminal.</summary>
    bool IsInteractive { get; }

    /// <summary>Whether the prompt input is currently locked.</summary>
    bool IsInputLocked { get; }

    /// <summary>Locks the prompt input.</summary>
    void LockInput();

    /// <summary>Unlocks the prompt input.</summary>
    void UnlockInput();

    /// <summary>Updates the echoed input line.</summary>
    void UpdateInput(string input);

    /// <summary>Writes a command output line above the prompt.</summary>
    void WriteLine(string line);

    /// <summary>Writes a formatted log line above the prompt.</summary>
    void WriteLogLine(string line, LogEventLevel level);
}
