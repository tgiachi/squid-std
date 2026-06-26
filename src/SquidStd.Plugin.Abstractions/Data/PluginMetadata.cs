namespace SquidStd.Plugin.Abstractions.Data;

/// <summary>
///     Describes a Moongate plugin. This is the source of truth for plugin identity.
/// </summary>
public sealed class PluginMetadata
{
    /// <summary>Stable lowercase dotted plugin identifier, for example <c>moongate.weather</c>.</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable plugin name.</summary>
    public required string Name { get; init; }

    /// <summary>Plugin version.</summary>
    public required Version Version { get; init; }

    /// <summary>Plugin author.</summary>
    public required string Author { get; init; }

    /// <summary>Optional human-readable description.</summary>
    public string? Description { get; init; }

    /// <summary>Plugin IDs that must load before this plugin.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = [];
}
