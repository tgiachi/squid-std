using Microsoft.CodeAnalysis;

namespace SquidStd.Generators.ConsoleCommands;

internal sealed class ConsoleCommandRegistrationCandidate
{
    public string ExecutorTypeName { get; }

    public string CommandName { get; }

    public string Description { get; }

    public string DisplayName { get; }

    public Location? Location { get; }

    public bool IsSupported { get; }

    public ConsoleCommandRegistrationCandidate(
        string executorTypeName,
        string commandName,
        string description,
        string displayName,
        Location? location,
        bool isSupported
    )
    {
        ExecutorTypeName = executorTypeName;
        CommandName = commandName;
        Description = description;
        DisplayName = displayName;
        Location = location;
        IsSupported = isSupported;
    }
}
