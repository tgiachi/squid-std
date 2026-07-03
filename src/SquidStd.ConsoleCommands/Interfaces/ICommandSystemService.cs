using SquidStd.ConsoleCommands.Data;

namespace SquidStd.ConsoleCommands.Interfaces;

/// <summary>
/// Registers and dispatches interactive console commands.
/// </summary>
public interface ICommandSystemService
{
    /// <summary>Registers one command or multiple aliases separated by <c>|</c>.</summary>
    /// <param name="commandName">Primary name or alias list, e.g. <c>"gc|collect"</c>.</param>
    /// <param name="handler">Handler invoked with the parsed context.</param>
    /// <param name="description">Help description.</param>
    /// <param name="autocompleteProvider">Optional argument suggestions for the current line.</param>
    void RegisterCommand(
        string commandName,
        Func<ConsoleCommandContext, Task> handler,
        string description = "",
        Func<string, IReadOnlyList<string>>? autocompleteProvider = null
    );

    /// <summary>Executes a raw command line; failures are reported, never thrown.</summary>
    Task ExecuteCommandAsync(string commandWithArgs, CancellationToken cancellationToken = default);

    /// <summary>Executes a raw command line collecting the lines it writes.</summary>
    Task<IReadOnlyList<string>> ExecuteCommandWithOutputAsync(
        string commandWithArgs,
        CancellationToken cancellationToken = default
    );

    /// <summary>Gets suggestions for the current input line.</summary>
    IReadOnlyList<string> GetAutocompleteSuggestions(string commandWithArgs);

    /// <summary>Gets the registered command definitions.</summary>
    IReadOnlyList<ConsoleCommandDefinition> GetRegisteredCommands();
}
