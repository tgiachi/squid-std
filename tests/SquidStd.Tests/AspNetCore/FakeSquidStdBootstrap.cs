using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Bootstrap;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Core.Types.Bootstrap;

namespace SquidStd.Tests.AspNetCore;

internal sealed class FakeSquidStdBootstrap : ISquidStdBootstrap
{
    public int StartCount { get; private set; }

    public int StopCount { get; private set; }

    public int ConfigureLoggingCount { get; private set; }

    public SquidStdOptions Options { get; } = new();

    public IContainer Container { get; } = new Container();

    public BootstrapStateType State { get; set; } = BootstrapStateType.Created;

    public ISquidStdBootstrap ConfigureService(Func<IContainer, IContainer> configure)
        => ConfigureServices(configure);

    public ISquidStdBootstrap ConfigureServices(Func<IContainer, IContainer> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var configuredContainer = configure(Container);

        return ReferenceEquals(configuredContainer, Container)
                   ? this
                   : throw new InvalidOperationException("ConfigureServices must return the bootstrap container instance.");
    }

    public ISquidStdBootstrap OnConfigLoaded<TConfig>(Action<TConfig> configure) where TConfig : class
    {
        ArgumentNullException.ThrowIfNull(configure);

        return this;
    }

    public ISquidStdBootstrap OnConfigReady(Action<IConfigManagerService> ready)
    {
        ArgumentNullException.ThrowIfNull(ready);

        return this;
    }

    public ValueTask DisposeAsync()
    {
        Container.Dispose();

        return ValueTask.CompletedTask;
    }

    public TService Resolve<TService>()
        => Container.Resolve<TService>();

    public void ConfigureLogging()
        => ConfigureLoggingCount++;

    public async Task RunAsync(CancellationToken cancellationToken = default)
        => await StartAsync(cancellationToken);

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StartCount++;
        State = BootstrapStateType.Started;

        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StopCount++;
        State = BootstrapStateType.Stopped;

        return ValueTask.CompletedTask;
    }
}
