using System.Collections.Concurrent;
using Serilog;
using SquidStd.ConsoleCommands.Data;
using SquidStd.ConsoleCommands.Interfaces;
using SquidStd.ConsoleCommands.Internal;

namespace SquidStd.ConsoleCommands.Services;

/// <summary>
/// Default in-memory implementation of <see cref="ICommandSystemService" />.
/// </summary>
public sealed class CommandSystemService : ICommandSystemService
{
    private readonly ILogger _logger = Log.ForContext<CommandSystemService>();

    private readonly ConcurrentDictionary<string, CommandEntry> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Action<string> _writeLine;

    /// <summary>Initializes the command system.</summary>
    /// <param name="writeLine">Writer used for command output and dispatch messages.</param>
    public CommandSystemService(Action<string> writeLine)
    {
        ArgumentNullException.ThrowIfNull(writeLine);

        _writeLine = writeLine;
    }

    /// <inheritdoc />
    public void RegisterCommand(
        string commandName,
        Func<ConsoleCommandContext, Task> handler,
        string description = "",
        Func<string, IReadOnlyList<string>>? autocompleteProvider = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(handler);

        var aliases = commandName.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var entry = new CommandEntry(
            aliases[0],
            aliases.Skip(1).ToArray(),
            description,
            handler,
            autocompleteProvider
        );

        foreach (var alias in aliases)
        {
            if (!_commands.TryAdd(alias, entry))
            {
                _logger.Warning("Console command '{Command}' redefined", alias);
                _commands[alias] = entry;
            }
        }
    }

    /// <inheritdoc />
    public Task ExecuteCommandAsync(string commandWithArgs, CancellationToken cancellationToken = default)
        => ExecuteCoreAsync(commandWithArgs, _writeLine, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ExecuteCommandWithOutputAsync(
        string commandWithArgs,
        CancellationToken cancellationToken = default
    )
    {
        var lines = new List<string>();

        await ExecuteCoreAsync(commandWithArgs, lines.Add, cancellationToken);

        return lines;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAutocompleteSuggestions(string commandWithArgs)
    {
        if (string.IsNullOrWhiteSpace(commandWithArgs))
        {
            return [];
        }

        if (!commandWithArgs.Contains(' '))
        {
            return _commands.Keys
                .Where(name => name.StartsWith(commandWithArgs, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var tokens = CommandLineTokenizer.Tokenize(commandWithArgs);

        if (tokens.Count == 0 || !_commands.TryGetValue(tokens[0], out var entry))
        {
            return [];
        }

        return entry.AutocompleteProvider?.Invoke(commandWithArgs) ?? [];
    }

    /// <inheritdoc />
    public IReadOnlyList<ConsoleCommandDefinition> GetRegisteredCommands()
        => _commands.Values
            .Distinct()
            .Select(entry => new ConsoleCommandDefinition(entry.Name, entry.Aliases, entry.Description))
            .OrderBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private async Task ExecuteCoreAsync(
        string commandWithArgs,
        Action<string> writeLine,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokens = CommandLineTokenizer.Tokenize(commandWithArgs);

        if (tokens.Count == 0)
        {
            return;
        }

        var name = tokens[0];

        if (!_commands.TryGetValue(name, out var entry))
        {
            writeLine($"Unknown command '{name}'. Type 'help' for the command list.");

            return;
        }

        var context = new ConsoleCommandContext(commandWithArgs, tokens.Skip(1).ToArray(), writeLine);

        try
        {
            await entry.Handler(context);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Console command '{Command}' failed", name);
            writeLine($"Command '{name}' failed: {ex.Message}");
        }
    }

    private sealed record CommandEntry(
        string Name,
        IReadOnlyList<string> Aliases,
        string Description,
        Func<ConsoleCommandContext, Task> Handler,
        Func<string, IReadOnlyList<string>>? AutocompleteProvider
    );
}
