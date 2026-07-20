using DryIoc;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Core.Config;
using SquidStd.Core.Interfaces.Config;

namespace SquidStd.Services.Core.Services;

/// <summary>
/// Adapter exposing the standalone <see cref="SquidStdConfig" /> through
/// <see cref="IConfigManagerService" />. Sections bind at registration time; <see cref="Load" />
/// performs an explicit reload.
/// </summary>
public sealed class ConfigManagerService : IConfigManagerService, ISquidStdService
{
    private readonly SquidStdConfig _config;
    private readonly IContainer _container;

    /// <inheritdoc />
    public string ConfigName => _config.ConfigName;

    /// <inheritdoc />
    public string ConfigDirectory => _config.ConfigDirectory;

    /// <inheritdoc />
    public string ConfigPath => _config.ConfigPath;

    /// <inheritdoc />
    public IReadOnlyCollection<IConfigEntry> Entries => _config.Entries;

    /// <inheritdoc />
    public event Action? ConfigLoaded;

    /// <summary>
    /// Initializes the adapter over the standalone configuration.
    /// </summary>
    /// <param name="config">The eagerly-loaded configuration.</param>
    /// <param name="container">Container holding the bound section instances.</param>
    public ConfigManagerService(SquidStdConfig config, IContainer container)
    {
        _config = config;
        _container = container;
    }

    /// <inheritdoc />
    public string Compose()
        => _config.Compose();

    /// <inheritdoc />
    public TConfig GetConfig<TConfig>() where TConfig : class
        => _container.Resolve<TConfig>();

    /// <inheritdoc />
    public void Load()
    {
        _config.Reload();
        Rebind(_config);

        foreach (var external in Externals())
        {
            external.Reload();
            Rebind(external);
        }

        ConfigLoaded?.Invoke();
    }

    /// <inheritdoc />
    public void Save()
    {
        _config.Save();

        foreach (var external in Externals())
        {
            external.Save();
        }
    }

    /// <inheritdoc />
    public void EnsureFiles()
    {
        SaveIfAbsent(_config);

        foreach (var external in Externals())
        {
            SaveIfAbsent(external);
        }
    }

    private void Rebind(SquidStdConfig config)
    {
        foreach (var (_, configType, instance) in config.BindAll())
        {
            _container.RegisterInstance(configType, instance, IfAlreadyRegistered.Replace);
        }
    }

    private IReadOnlyCollection<SquidStdConfig> Externals()
        => _container.Resolve<ExternalConfigRegistry>(IfUnresolved.ReturnDefault)?.All ?? [];

    private static void SaveIfAbsent(SquidStdConfig config)
    {
        if (!File.Exists(config.ConfigPath))
        {
            config.Save();
        }
    }

    /// <inheritdoc />
    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.CompletedTask;
    }
}
