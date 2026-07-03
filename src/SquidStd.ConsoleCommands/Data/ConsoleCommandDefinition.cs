namespace SquidStd.ConsoleCommands.Data;

/// <summary>
/// Describes a registered console command.
/// </summary>
/// <param name="Name">Primary command name.</param>
/// <param name="Aliases">Additional aliases.</param>
/// <param name="Description">Help description.</param>
public sealed record ConsoleCommandDefinition(string Name, IReadOnlyList<string> Aliases, string Description);
