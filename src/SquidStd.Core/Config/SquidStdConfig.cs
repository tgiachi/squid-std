using SquidStd.Core.Config.Internal;
using SquidStd.Core.Extensions.Directories;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Core.Yaml;

namespace SquidStd.Core.Config;

/// <summary>
/// Standalone YAML configuration loader. Loads the raw document eagerly and binds sections on
/// demand, outside any dependency-injection container. Instances are cached per section and
/// environment-variable substitution is applied at bind time.
/// </summary>
public sealed class SquidStdConfig
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, ConfigSectionEntry> _sections = new(StringComparer.Ordinal);
    private string _rawYaml;

    /// <summary>Gets the logical configuration name.</summary>
    public string ConfigName { get; }

    /// <summary>Gets the fully-resolved configuration directory.</summary>
    public string ConfigDirectory { get; }

    /// <summary>Gets the full path of the YAML configuration file.</summary>
    public string ConfigPath { get; }

    /// <summary>Gets the tracked sections (bound and unbound), ordered by priority then name.</summary>
    public IReadOnlyCollection<IConfigEntry> Entries
    {
        get
        {
            lock (_sync)
            {
                return OrderedEntries().Cast<IConfigEntry>().ToArray();
            }
        }
    }

    private SquidStdConfig(string configName, string configDirectory, string rawYaml)
    {
        ConfigName = configName;
        ConfigDirectory = configDirectory;
        ConfigPath = ResolveConfigPath(configName, configDirectory);
        _rawYaml = rawYaml;
    }

    /// <summary>
    /// Loads the configuration file eagerly. A missing file yields an empty document (defaults);
    /// the file is not created. Tilde and environment variables in
    /// <paramref name="configDirectory" /> are resolved.
    /// </summary>
    /// <param name="configName">Logical configuration name or YAML file name.</param>
    /// <param name="configDirectory">Directory where the configuration file is searched.</param>
    /// <returns>The loaded configuration.</returns>
    public static SquidStdConfig Load(string configName, string configDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configName);
        ArgumentException.ThrowIfNullOrWhiteSpace(configDirectory);

        var resolvedDirectory = Path.GetFullPath(configDirectory.ResolvePathAndEnvs());
        var path = ResolveConfigPath(configName, resolvedDirectory);
        var raw = File.Exists(path) ? File.ReadAllText(path) : string.Empty;

        return new(configName, resolvedDirectory, raw);
    }

    /// <summary>
    /// Binds the named section (YAML content or defaults) and caches it: repeated calls return
    /// the same instance. Environment variables in string properties are substituted.
    /// </summary>
    /// <typeparam name="TConfig">The section type.</typeparam>
    /// <param name="sectionName">The YAML section name.</param>
    /// <returns>The bound section instance.</returns>
    public TConfig GetSection<TConfig>(string sectionName)
        where TConfig : class, new()
        => BindSection<TConfig>(sectionName, null, 0);

    /// <summary>
    /// Binds the named section with an explicit default factory and Compose ordering priority.
    /// Used by the registration extensions; behaves like <see cref="GetSection{TConfig}" />.
    /// </summary>
    /// <typeparam name="TConfig">The section type.</typeparam>
    /// <param name="sectionName">The YAML section name.</param>
    /// <param name="createDefault">Factory used when the section is absent from the file.</param>
    /// <param name="priority">Ordering priority for Compose/Save.</param>
    /// <returns>The bound section instance.</returns>
    public TConfig BindSection<TConfig>(string sectionName, Func<TConfig>? createDefault, int priority)
        where TConfig : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        lock (_sync)
        {
            var entry = EnsureEntry(
                sectionName,
                typeof(TConfig),
                priority,
                createDefault is null ? static () => new TConfig() : () => createDefault()
            );

            entry.Instance ??= Bind(entry);

            return (TConfig)entry.Instance;
        }
    }

    /// <summary>
    /// Records a section for Compose/Save without binding it. Infrastructure API used by the
    /// registration extensions.
    /// </summary>
    /// <param name="sectionName">The YAML section name.</param>
    /// <param name="configType">The section CLR type.</param>
    /// <param name="priority">Ordering priority for Compose/Save.</param>
    /// <param name="createDefault">Factory producing the default instance.</param>
    public void TrackSection(string sectionName, Type configType, int priority, Func<object> createDefault)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentNullException.ThrowIfNull(configType);
        ArgumentNullException.ThrowIfNull(createDefault);

        lock (_sync)
        {
            EnsureEntry(sectionName, configType, priority, createDefault);
        }
    }

    /// <summary>
    /// True when the raw document contains the named top-level section.
    /// </summary>
    /// <param name="sectionName">The YAML section name.</param>
    public bool HasSection(string sectionName)
    {
        lock (_sync)
        {
            return !string.IsNullOrWhiteSpace(_rawYaml) &&
                   YamlUtils.DeserializeSection(_rawYaml, sectionName, typeof(Dictionary<object, object>)) is not null;
        }
    }

    /// <summary>
    /// Re-reads the configuration file and clears the bind cache; tracked sections are kept and
    /// rebind lazily (or via <see cref="BindAll" />).
    /// </summary>
    public void Reload()
    {
        lock (_sync)
        {
            _rawYaml = File.Exists(ConfigPath) ? File.ReadAllText(ConfigPath) : string.Empty;

            foreach (var entry in _sections.Values)
            {
                entry.Instance = null;
            }
        }
    }

    /// <summary>
    /// Binds every tracked section and returns the bound instances (priority order).
    /// </summary>
    public IReadOnlyList<(string SectionName, Type ConfigType, object Instance)> BindAll()
    {
        lock (_sync)
        {
            var result = new List<(string, Type, object)>(_sections.Count);

            foreach (var entry in OrderedEntries())
            {
                entry.Instance ??= Bind(entry);
                result.Add((entry.SectionName, entry.ConfigType, entry.Instance));
            }

            return result;
        }
    }

    /// <summary>
    /// Serializes every tracked section (bound instances, or defaults for unbound ones).
    /// </summary>
    public string Compose()
    {
        lock (_sync)
        {
            return YamlUtils.SerializeSections(BuildSectionMap());
        }
    }

    /// <summary>
    /// Writes the composed configuration to <see cref="ConfigPath" />.
    /// </summary>
    public void Save()
    {
        lock (_sync)
        {
            YamlUtils.SerializeToFile(BuildSectionMap(), ConfigPath);
        }
    }

    private ConfigSectionEntry EnsureEntry(string sectionName, Type configType, int priority, Func<object> createDefault)
    {
        if (_sections.TryGetValue(sectionName, out var existing))
        {
            if (existing.ConfigType != configType)
            {
                throw new InvalidOperationException($"Config section '{sectionName}' is already registered.");
            }

            return existing;
        }

        if (_sections.Values.Any(entry => entry.ConfigType == configType))
        {
            throw new InvalidOperationException($"Config type '{configType.Name}' is already registered.");
        }

        var created = new ConfigSectionEntry(sectionName, configType, priority, createDefault);
        _sections[sectionName] = created;

        return created;
    }

    private object Bind(ConfigSectionEntry entry)
    {
        var value = string.IsNullOrWhiteSpace(_rawYaml)
                        ? entry.CreateDefault()
                        : YamlUtils.DeserializeSection(_rawYaml, entry.SectionName, entry.ConfigType) ??
                          entry.CreateDefault();

        ConfigEnvSubstitution.Apply(value);

        return value;
    }

    private Dictionary<string, object> BuildSectionMap()
    {
        var sections = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (var entry in OrderedEntries())
        {
            sections[entry.SectionName] = entry.Instance ?? entry.CreateDefault();
        }

        return sections;
    }

    private IEnumerable<ConfigSectionEntry> OrderedEntries()
        => _sections.Values
                    .OrderBy(entry => entry.Priority)
                    .ThenBy(entry => entry.SectionName, StringComparer.Ordinal);

    private static string ResolveConfigPath(string configName, string configDirectory)
    {
        var fileName = Path.HasExtension(configName) ? configName : $"{configName}.yaml";

        return Path.Combine(configDirectory, fileName);
    }
}
