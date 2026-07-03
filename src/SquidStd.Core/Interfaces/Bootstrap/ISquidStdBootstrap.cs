using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Config;

namespace SquidStd.Core.Interfaces.Bootstrap;

/// <summary>
/// Coordinates SquidStd dependency registration, configuration loading, and service lifecycle.
/// </summary>
public interface ISquidStdBootstrap : IAsyncDisposable
{
    /// <summary>
    /// Gets the bootstrap options used to configure runtime resources.
    /// </summary>
    SquidStdOptions Options { get; }

    /// <summary>
    /// Gets the owned dependency injection container.
    /// </summary>
    IContainer Container { get; }

    /// <summary>
    /// Applies custom service registrations before the bootstrap lifecycle starts.
    /// </summary>
    /// <param name="configure">Callback that receives and returns the container.</param>
    /// <returns>The same bootstrap instance for chaining.</returns>
    ISquidStdBootstrap ConfigureService(Func<IContainer, IContainer> configure);

    /// <summary>
    /// Applies custom service registrations before the bootstrap lifecycle starts.
    /// </summary>
    /// <param name="configure">Callback that receives and returns the container.</param>
    /// <returns>The same bootstrap instance for chaining.</returns>
    ISquidStdBootstrap ConfigureServices(Func<IContainer, IContainer> configure);

    /// <summary>
    /// Registers a callback invoked after the configuration is loaded (and re-applied on every
    /// reload performed through the bootstrap) and before the logger and services consume it.
    /// The callback receives the section singleton and may inspect or mutate it; changes are
    /// in-memory only - the YAML file is not rewritten.
    /// </summary>
    /// <typeparam name="TConfig">The configuration section type.</typeparam>
    /// <param name="configure">Callback that receives the loaded section.</param>
    /// <returns>The same bootstrap for chaining.</returns>
    ISquidStdBootstrap OnConfigLoaded<TConfig>(Action<TConfig> configure) where TConfig : class;

    /// <summary>
    /// Registers a callback invoked with the whole configuration manager after every load, once
    /// all <see cref="OnConfigLoaded{TConfig}" /> hooks have been applied and before the logger
    /// and services consume the sections. Use it to inspect (or dump via
    /// <see cref="IConfigManagerService.Compose" />) the final configuration.
    /// </summary>
    /// <param name="ready">Callback that receives the configuration manager.</param>
    /// <returns>The same bootstrap for chaining.</returns>
    ISquidStdBootstrap OnConfigReady(Action<IConfigManagerService> ready);

    /// <summary>
    /// Resolves a service from the owned dependency injection container.
    /// </summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    TService Resolve<TService>();

    /// <summary>
    /// Loads configuration and configures the Serilog logger immediately, if not already configured.
    /// Idempotent: subsequent calls are no-ops. Lets ASP.NET hosts wire Serilog before the host starts.
    /// </summary>
    void ConfigureLogging();

    /// <summary>
    /// Starts services, waits until cancellation, and then stops services.
    /// </summary>
    /// <param name="cancellationToken">Token that controls the run lifetime.</param>
    /// <returns>A task that completes after services have stopped.</returns>
    Task RunAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts registered lifecycle services in priority order.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the start operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    ValueTask StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops started lifecycle services in reverse priority order.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the stop operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
