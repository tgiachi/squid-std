using System.Diagnostics;
using System.Reflection;
using DryIoc;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SquidStd.Abstractions.Data.Internal.Config;
using SquidStd.Abstractions.Data.Internal.Services;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Extensions.Logger;
using SquidStd.Core.Interfaces.Bootstrap;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Core.Types;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Extensions.Logger;
using SquidStd.Services.Core.Types;

namespace SquidStd.Services.Core.Services.Bootstrap;

/// <summary>
/// Default SquidStd bootstrapper and service lifecycle orchestrator.
/// </summary>
public sealed class SquidStdBootstrap : ISquidStdBootstrap
{
    private readonly List<(Type ConfigType, Action<object> Apply)> _configHooks = [];
    private readonly bool _ownsContainer;
    private readonly List<ISquidStdService> _startedServices = [];
    private readonly Lock _syncRoot = new();
    private bool _configHooksSubscribed;
    private int _disposed;
    private bool _loggerConfigured;
    private BootstrapStateType _state;

    /// <inheritdoc />
    public SquidStdOptions Options { get; }

    /// <inheritdoc />
    public IContainer Container { get; }

    /// <summary>
    /// Initializes a bootstrapper with default options.
    /// </summary>
    public SquidStdBootstrap()
        : this(new()) { }

    /// <summary>
    /// Initializes a bootstrapper with the specified options.
    /// </summary>
    /// <param name="options">Bootstrap options used to register core services.</param>
    public SquidStdBootstrap(SquidStdOptions options)
        : this(options, new Container(), true) { }

    private SquidStdBootstrap(SquidStdOptions options, IContainer container, bool ownsContainer)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConfigName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.RootDirectory);

        Options = options;
        Container = container;
        _ownsContainer = ownsContainer;

        Container.RegisterInstance<ISquidStdBootstrap>(this, IfAlreadyRegistered.Replace);
        Container.RegisterInstance(this, IfAlreadyRegistered.Replace);
        Container.RegisterInstance(Options, IfAlreadyRegistered.Replace);
        Container.RegisterConfigServices(Options.ConfigName, Options.RootDirectory);
    }

    /// <inheritdoc />
    public ISquidStdBootstrap ConfigureService(Func<IContainer, IContainer> configure)
        => ConfigureServices(configure);

    /// <inheritdoc />
    public ISquidStdBootstrap ConfigureServices(Func<IContainer, IContainer> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ThrowIfDisposed();

        lock (_syncRoot)
        {
            if (_state != BootstrapStateType.Created)
            {
                throw new InvalidOperationException("Services cannot be configured after bootstrap start.");
            }
        }

        var configuredContainer = configure(Container);

        return !ReferenceEquals(configuredContainer, Container)
                   ? throw new InvalidOperationException("ConfigureServices must return the bootstrap container instance.")
                   : this;
    }

    /// <inheritdoc />
    public ISquidStdBootstrap OnConfigLoaded<TConfig>(Action<TConfig> configure) where TConfig : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        ThrowIfDisposed();

        lock (_syncRoot)
        {
            if (_state != BootstrapStateType.Created)
            {
                throw new InvalidOperationException("Config hooks cannot be registered after bootstrap start.");
            }
        }

        _configHooks.Add((typeof(TConfig), instance => configure((TConfig)instance)));

        return this;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            await StopAsync(CancellationToken.None);
        }
        finally
        {
            if (_loggerConfigured)
            {
                await Log.CloseAndFlushAsync();
            }

            if (_ownsContainer)
            {
                Container.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public TService Resolve<TService>()
    {
        ThrowIfDisposed();

        return Container.Resolve<TService>();
    }

    /// <inheritdoc />
    public void ConfigureLogging()
    {
        ThrowIfDisposed();

        if (_loggerConfigured)
        {
            return;
        }

        var configManager = Container.Resolve<IConfigManagerService>();

        if (!_configHooksSubscribed)
        {
            configManager.ConfigLoaded += ApplyConfigHooks;
            _configHooksSubscribed = true;
        }

        configManager.Load();
        ConfigureLogger();
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await StartAsync(cancellationToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            await StopAsync(CancellationToken.None);
        }
    }

    /// <inheritdoc />
    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        MarkStarting();

        try
        {
            ConfigureLogging();

            var logger = Log.ForContext<SquidStdBootstrap>();
            var appName = ResolveAppName(Options);

            logger.Information(
                "{Application:l} {ApplicationVersion:l} starting (SquidStd {SquidStdVersion:l}, config {ConfigName}, root {RootDirectory})",
                appName,
                ResolveAppVersion(Options),
                ResolveVersion(typeof(SquidStdBootstrap).Assembly),
                Options.ConfigName,
                Options.RootDirectory
            );

            var registrations = GetServiceRegistrations();
            LogRegistrations(logger, registrations);

            var startedInstances = new HashSet<ISquidStdService>(ReferenceEqualityComparer.Instance);
            var totalStopwatch = Stopwatch.StartNew();

            for (var i = 0; i < registrations.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var registration = registrations[i];
                var instance = Container.Resolve(registration.ServiceType);

                if (instance is not ISquidStdService service || !startedInstances.Add(service))
                {
                    continue;
                }

                var serviceStopwatch = Stopwatch.StartNew();

                try
                {
                    await service.StartAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Service {Service:l} failed to start", service.GetType().Name);

                    throw;
                }

                _startedServices.Add(service);
                logger.Information(
                    "Started {Service:l} in {Elapsed:0.#}ms",
                    service.GetType().Name,
                    serviceStopwatch.Elapsed.TotalMilliseconds
                );
            }

            logger.Information(
                "{Application:l} started: {Count} service(s) in {TotalElapsed:0.#}ms",
                appName,
                _startedServices.Count,
                totalStopwatch.Elapsed.TotalMilliseconds
            );
        }
        catch
        {
            MarkCreated();

            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (!MarkStopping())
        {
            return;
        }

        var logger = Log.ForContext<SquidStdBootstrap>();
        var appName = ResolveAppName(Options);
        logger.Information("{Application:l} stopping ({Count} service(s))", appName, _startedServices.Count);

        try
        {
            for (var i = _startedServices.Count - 1; i >= 0; i--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var service = _startedServices[i];

                try
                {
                    await service.StopAsync(cancellationToken);
                    logger.Information("Stopped {Service:l}", service.GetType().Name);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, "Service {Service:l} failed to stop", service.GetType().Name);
                }
            }

            logger.Information("{Application:l} shutdown complete", appName);
        }
        finally
        {
            _startedServices.Clear();
        }
    }

    /// <summary>
    /// Creates a bootstrapper with default options.
    /// </summary>
    /// <returns>The created bootstrapper.</returns>
    public static SquidStdBootstrap Create()
        => new();

    /// <summary>
    /// Creates a bootstrapper with the specified options.
    /// </summary>
    /// <param name="options">Bootstrap options used to register core services.</param>
    /// <returns>The created bootstrapper.</returns>
    public static SquidStdBootstrap Create(SquidStdOptions options)
        => new(options);

    /// <summary>
    /// Creates a bootstrapper using an externally owned DryIoc container.
    /// </summary>
    /// <param name="options">Bootstrap options used to register core services.</param>
    /// <param name="container">Externally owned container that receives SquidStd services.</param>
    /// <returns>The created bootstrapper.</returns>
    public static SquidStdBootstrap Create(SquidStdOptions options, IContainer container)
        => new(options, container, false);

    /// <summary>
    /// Creates a bootstrapper configuring the options through a callback.
    /// </summary>
    /// <param name="configure">Callback that mutates the default <see cref="SquidStdOptions" />.</param>
    /// <returns>The configured bootstrapper.</returns>
    public static SquidStdBootstrap Create(Action<SquidStdOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SquidStdOptions();
        configure(options);

        return Create(options);
    }

    private static string ResolveAppName(SquidStdOptions options)
        => !string.IsNullOrWhiteSpace(options.AppName)
               ? options.AppName
               : Assembly.GetEntryAssembly()?.GetName().Name ?? "SquidStd";

    private static string ResolveAppVersion(SquidStdOptions options)
        => !string.IsNullOrWhiteSpace(options.AppVersion)
               ? options.AppVersion
               : ResolveVersion(Assembly.GetEntryAssembly());

    private static string ResolveVersion(Assembly? assembly)
    {
        var informational = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                    ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
        {
            return assembly?.GetName().Version?.ToString() ?? "0.0.0";
        }

        var metadataStart = informational.IndexOf('+');

        return metadataStart > 0 ? informational[..metadataStart] : informational;
    }

    private void ConfigureLogger()
    {
        if (_loggerConfigured || !Container.IsRegistered<SquidStdLoggerOptions>())
        {
            return;
        }

        var options = Container.Resolve<SquidStdLoggerOptions>();
        var loggerConfiguration = new LoggerConfiguration();

        loggerConfiguration
            .Enrich.WithProperty("Application", ResolveAppName(Options))
            .Enrich.WithProperty("ApplicationVersion", ResolveAppVersion(Options));

        if (options.MinimumLevel != LogLevelType.None)
        {
            var minimumLevel = options.MinimumLevel.ToSerilogLogLevel();
            loggerConfiguration.MinimumLevel.Is(minimumLevel);

            if (options.EnableConsole)
            {
                loggerConfiguration.WriteTo.Console(minimumLevel);
            }

            if (options.EnableFile)
            {
                loggerConfiguration.WriteTo.File(
                    ResolveLogPath(options),
                    rollingInterval: options.RollingInterval.ToSerilogRollingInterval(),
                    restrictedToMinimumLevel: minimumLevel
                );
            }

            foreach (var sink in Container.ResolveMany<ILogEventSink>())
            {
                loggerConfiguration.WriteTo.Sink(sink, minimumLevel);
            }
        }

        var logger = loggerConfiguration.CreateLogger();
        Log.Logger = logger;
        Container.RegisterInstance<ILogger>(logger, IfAlreadyRegistered.Replace);
        _loggerConfigured = true;
    }

    private void ApplyConfigHooks()
    {
        if (_configHooks.Count == 0)
        {
            return;
        }

        List<ConfigRegistrationData> sections = Container.IsRegistered<List<ConfigRegistrationData>>()
                                                    ? Container.Resolve<List<ConfigRegistrationData>>()
                                                    : [];
        var logger = Log.ForContext<SquidStdBootstrap>();

        foreach (var (configType, apply) in _configHooks)
        {
            if (!sections.Any(section => section.ConfigType == configType))
            {
                throw new InvalidOperationException(
                    $"No config section registered for type '{configType.Name}'. "
                    + "Register it with RegisterConfigSection before using OnConfigLoaded."
                );
            }

            apply(Container.Resolve(configType));
            logger.Debug("Applied config hook to {Section:l}", configType.Name);
        }
    }

    private void LogRegistrations(ILogger logger, ServiceRegistrationData[] lifecycleRegistrations)
    {
        List<ConfigRegistrationData> sections = Container.IsRegistered<List<ConfigRegistrationData>>()
                                                    ? Container.Resolve<List<ConfigRegistrationData>>()
                                                    : [];
        var containerRegistrations = Container.GetServiceRegistrations().ToArray();

        logger.Information(
            "Registered {LifecycleCount} lifecycle service(s), {SectionCount} config section(s), {ContainerCount} container registration(s)",
            lifecycleRegistrations.Length,
            sections.Count,
            containerRegistrations.Length
        );

        if (!logger.IsEnabled(LogEventLevel.Debug))
        {
            return;
        }

        foreach (var registration in lifecycleRegistrations)
        {
            logger.Debug(
                "Lifecycle service {Service} -> {Implementation} (priority {Priority})",
                registration.ServiceType.Name,
                registration.ImplementationType.Name,
                registration.Priority
            );
        }

        foreach (var section in sections)
        {
            logger.Debug("Config section '{Section}' -> {Type}", section.SectionName, section.ConfigType.Name);
        }

        foreach (var group in containerRegistrations
                     .GroupBy(registration => registration.ServiceType.Assembly.GetName().Name ?? "unknown")
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            logger.Debug(
                "{Assembly}: {Count} registration(s): {Services}",
                group.Key,
                group.Count(),
                string.Join(
                    ", ",
                    group.Select(registration => registration.ServiceType.Name)
                         .Distinct()
                         .Order(StringComparer.Ordinal)
                )
            );
        }
    }

    private ServiceRegistrationData[] GetServiceRegistrations()
    {
        if (!Container.IsRegistered<List<ServiceRegistrationData>>())
        {
            return [];
        }

        return
        [
            .. Container.Resolve<List<ServiceRegistrationData>>()
                        .OrderBy(registration => registration.Priority)
                        .ThenBy(registration => registration.ServiceType.FullName, StringComparer.Ordinal)
                        .ThenBy(registration => registration.ImplementationType.FullName, StringComparer.Ordinal)
        ];
    }

    private void MarkCreated()
    {
        lock (_syncRoot)
        {
            _state = BootstrapStateType.Created;
        }
    }

    private void MarkStarting()
    {
        lock (_syncRoot)
        {
            if (_state == BootstrapStateType.Started)
            {
                return;
            }

            if (_state == BootstrapStateType.Stopped)
            {
                throw new InvalidOperationException("Bootstrap cannot be restarted after stop.");
            }

            _state = BootstrapStateType.Started;
        }
    }

    private bool MarkStopping()
    {
        lock (_syncRoot)
        {
            if (_state == BootstrapStateType.Stopped)
            {
                return false;
            }

            if (_state == BootstrapStateType.Created)
            {
                _state = BootstrapStateType.Stopped;

                return false;
            }

            _state = BootstrapStateType.Stopped;

            return true;
        }
    }

    private string ResolveLogPath(SquidStdLoggerOptions options)
    {
        var logDirectory = string.IsNullOrWhiteSpace(options.LogDirectory) ? "logs" : options.LogDirectory;
        var fileName = string.IsNullOrWhiteSpace(options.FileName) ? "squidstd-.log" : options.FileName;
        var directory = Path.IsPathRooted(logDirectory)
                            ? logDirectory
                            : Path.Combine(Options.RootDirectory, logDirectory);

        return Path.Combine(directory, fileName);
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(SquidStdBootstrap));
        }
    }
}
