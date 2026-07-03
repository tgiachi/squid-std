using DryIoc;
using SquidStd.ConsoleCommands.Internal;
using SquidStd.ConsoleCommands.Services;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Bootstrap;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Services.Core.Services.Lifecycle;

namespace SquidStd.Tests.ConsoleCommands;

public class BuiltinCommandsTests
{
    [Fact]
    public async Task Help_ListsRegisteredCommandsWithDescriptions()
    {
        var lines = new List<string>();
        var system = new CommandSystemService(lines.Add);
        BuiltinConsoleCommands.Register(system, lifetime: null, bootstrap: null, clearScreen: () => { });
        system.RegisterCommand("custom", _ => Task.CompletedTask, "does things");

        var output = await system.ExecuteCommandWithOutputAsync("help");

        Assert.Contains(output, line => line.Contains("custom", StringComparison.Ordinal)
                                        && line.Contains("does things", StringComparison.Ordinal));
        Assert.Contains(output, line => line.Contains("help", StringComparison.Ordinal));
        Assert.Contains(output, line => line.Contains("exit", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Exit_WithoutBootstrap_ReportsUnavailable()
    {
        var system = new CommandSystemService(_ => { });
        BuiltinConsoleCommands.Register(system, lifetime: null, bootstrap: null, clearScreen: () => { });

        var output = await system.ExecuteCommandWithOutputAsync("exit");

        Assert.Contains(output, line => line.Contains("not available", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Exit_WithBootstrap_RequestsStop()
    {
        var system = new CommandSystemService(_ => { });
        var stopSignal = new TaskCompletionSource();
        BuiltinConsoleCommands.Register(
            system,
            lifetime: null,
            bootstrap: new StopSpyBootstrap(() => stopSignal.TrySetResult()),
            clearScreen: () => { }
        );

        await system.ExecuteCommandAsync("exit");
        var stopped = await Task.WhenAny(stopSignal.Task, Task.Delay(2000)) == stopSignal.Task;

        Assert.True(stopped);
    }

    [Fact]
    public async Task Exit_WithLifetime_RequestsShutdown_WithoutStoppingBootstrapDirectly()
    {
        var system = new CommandSystemService(_ => { });
        using var lifetime = new SquidStdLifetimeService();
        var bootstrapStopped = false;
        BuiltinConsoleCommands.Register(
            system,
            lifetime: lifetime,
            bootstrap: new StopSpyBootstrap(() => bootstrapStopped = true),
            clearScreen: () => { }
        );

        await system.ExecuteCommandAsync("exit");
        await Task.Delay(100);

        Assert.True(lifetime.ShutdownToken.IsCancellationRequested);
        Assert.False(bootstrapStopped);
    }

    private sealed class StopSpyBootstrap : ISquidStdBootstrap
    {
        private readonly Action _onStop;

        public SquidStdOptions Options { get; } = new();

        public IContainer Container { get; } = new Container();

        public StopSpyBootstrap(Action onStop)
        {
            _onStop = onStop;
        }

        public ISquidStdBootstrap ConfigureService(Func<IContainer, IContainer> configure)
            => this;

        public ISquidStdBootstrap ConfigureServices(Func<IContainer, IContainer> configure)
            => this;

        public ISquidStdBootstrap OnConfigLoaded<TConfig>(Action<TConfig> configure) where TConfig : class
            => this;

        public ISquidStdBootstrap OnConfigReady(Action<IConfigManagerService> ready)
            => this;

        public TService Resolve<TService>()
            => Container.Resolve<TService>();

        public void ConfigureLogging()
        {
        }

        public Task RunAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public ValueTask StartAsync(CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public ValueTask StopAsync(CancellationToken cancellationToken = default)
        {
            _onStop();

            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Container.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
