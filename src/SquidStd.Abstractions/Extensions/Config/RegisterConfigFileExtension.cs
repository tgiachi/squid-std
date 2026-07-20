using DryIoc;
using Serilog;
using SquidStd.Core.Config;

namespace SquidStd.Abstractions.Extensions.Config;

/// <summary>
/// Registers a config section bound from its own external YAML file (rather than a section of the
/// primary document). Mirrors <see cref="RegisterConfigSectionExtension" />; the manager writes and
/// reloads external files alongside the primary.
/// </summary>
public static class RegisterConfigFileExtension
{
    public static IContainer RegisterConfigFile<TConfig>(
        this IContainer container,
        string sectionName,
        string configDirectory,
        string? configName = null,
        Func<TConfig>? createDefault = null,
        int priority = 0
    )
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(configDirectory);

        var configType = typeof(TConfig);

        // An explicit instance already registered wins (same guard as RegisterConfigSection).
        if (container.IsRegistered<TConfig>())
        {
            Log.ForContext(typeof(RegisterConfigFileExtension))
               .Debug("Config file section {Section:l} skipped: {ConfigType:l} already registered", sectionName, configType.Name);

            return container;
        }

        // Degenerate mode: no primary config → just register the default, no file.
        if (!container.IsRegistered<SquidStdConfig>())
        {
            container.RegisterInstance(createDefault?.Invoke() ?? new TConfig(), IfAlreadyRegistered.Keep);

            return container;
        }

        var name = string.IsNullOrWhiteSpace(configName) ? sectionName : configName;
        var convention = container.Resolve<SquidStdConfig>().NamingConvention;
        var path = Path.Combine(Path.GetFullPath(configDirectory), Path.HasExtension(name) ? name : $"{name}.yaml");

        var registry = EnsureRegistry(container);
        var external = registry.GetOrAdd(path, () => SquidStdConfig.Load(name, configDirectory, convention));

        var bound = external.BindSection(sectionName, createDefault, priority);
        container.RegisterInstance(bound, IfAlreadyRegistered.Replace);

        return container;
    }

    private static ExternalConfigRegistry EnsureRegistry(IContainer container)
    {
        if (container.IsRegistered<ExternalConfigRegistry>())
        {
            return container.Resolve<ExternalConfigRegistry>();
        }

        var registry = new ExternalConfigRegistry();
        container.RegisterInstance(registry);

        return registry;
    }
}
