using Serilog.Events;
using SquidStd.ConsoleCommands.Data.Config;
using SquidStd.ConsoleCommands.Interfaces;

namespace SquidStd.ConsoleCommands.Services;

/// <summary>
/// Console UI for the command prompt. In non-interactive environments (redirected streams)
/// every write is a plain <see cref="Console.WriteLine(string)" /> and input state is inert.
/// </summary>
public sealed class ConsoleUiService : IConsoleUiService
{
    private readonly ConsoleCommandsConfig _config;

    /// <inheritdoc />
    public bool IsInteractive { get; }

    /// <inheritdoc />
    public bool IsInputLocked { get; private set; }

    /// <summary>Initializes the console UI from the prompt configuration.</summary>
    /// <param name="config">Prompt configuration section.</param>
    public ConsoleUiService(ConsoleCommandsConfig config)
    {
        _config = config;
        IsInteractive = !Console.IsInputRedirected && !Console.IsOutputRedirected;
        IsInputLocked = config.StartLocked;
    }

    /// <inheritdoc />
    public void LockInput()
        => IsInputLocked = true;

    /// <inheritdoc />
    public void UnlockInput()
        => IsInputLocked = false;

    /// <inheritdoc />
    public void UpdateInput(string input)
    {
        // Non-interactive: no prompt row to render. The interactive branch lands with the input loop.
    }

    /// <inheritdoc />
    public void WriteLine(string line)
        => Console.WriteLine(line);

    /// <inheritdoc />
    public void WriteLogLine(string line, LogEventLevel level)
        => Console.WriteLine(line);
}
