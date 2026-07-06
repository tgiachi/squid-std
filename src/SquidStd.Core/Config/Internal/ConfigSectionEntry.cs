using SquidStd.Core.Interfaces.Config;

namespace SquidStd.Core.Config.Internal;

/// <summary>
/// Tracks a single configuration section registered with a <see cref="SquidStdConfig" /> instance,
/// including its bind priority and cached bound instance.
/// </summary>
internal sealed class ConfigSectionEntry : IConfigEntry
{
    private readonly Func<object> _createDefault;

    /// <inheritdoc />
    public string SectionName { get; }

    /// <inheritdoc />
    public Type ConfigType { get; }

    /// <summary>
    /// Gets the ordering priority used by <see cref="SquidStdConfig.Compose" /> and
    /// <see cref="SquidStdConfig.BindAll" />.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Gets or sets the cached bound instance, or <see langword="null" /> when not yet bound.
    /// </summary>
    public object? Instance { get; set; }

    /// <summary>
    /// Initializes a new section entry.
    /// </summary>
    /// <param name="sectionName">The top-level YAML section name.</param>
    /// <param name="configType">The section CLR type.</param>
    /// <param name="priority">Ordering priority for Compose/Save.</param>
    /// <param name="createDefault">Factory producing the default instance.</param>
    public ConfigSectionEntry(string sectionName, Type configType, int priority, Func<object> createDefault)
    {
        SectionName = sectionName;
        ConfigType = configType;
        Priority = priority;
        _createDefault = createDefault;
    }

    /// <inheritdoc />
    public object CreateDefault()
        => _createDefault();
}
