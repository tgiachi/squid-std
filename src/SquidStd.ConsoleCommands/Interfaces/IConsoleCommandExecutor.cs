using SquidStd.ConsoleCommands.Data;

namespace SquidStd.ConsoleCommands.Interfaces;

/// <summary>
/// Contract for attribute-registered console command classes.
/// </summary>
public interface IConsoleCommandExecutor
{
    /// <summary>Help description of the command.</summary>
    string Description { get; }

    /// <summary>Executes the command.</summary>
    Task ExecuteAsync(ConsoleCommandContext context);
}
