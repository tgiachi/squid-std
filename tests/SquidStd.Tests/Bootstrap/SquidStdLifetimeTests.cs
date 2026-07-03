using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Core.Interfaces.Lifecycle;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

[Collection(SerilogEventSinkCollection.Name)]
public class SquidStdLifetimeTests
{
    [Fact]
    public async Task RequestShutdown_CompletesRunAsync()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path, AppName = "LifetimeApp" }
        );
        bootstrap.ConfigureServices(c => c.RegisterCoreServices());

        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopped = false;
        var bus = bootstrap.Container.Resolve<IEventBus>();
        using var startedSubscription = bus.Subscribe<EngineStartedEvent>(
            (_, _) =>
            {
                started.TrySetResult();

                return Task.CompletedTask;
            }
        );
        using var stoppedSubscription = bus.Subscribe<EngineStoppedEvent>(
            (_, _) =>
            {
                stopped = true;

                return Task.CompletedTask;
            }
        );

        var runTask = bootstrap.RunAsync();

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        bootstrap.Container.Resolve<ISquidStdLifetime>().RequestShutdown();
        await runTask.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.True(runTask.IsCompletedSuccessfully);
        Assert.True(stopped);
    }

    [Fact]
    public async Task RequestShutdown_IsIdempotent()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path }
        );

        var lifetime = bootstrap.Container.Resolve<ISquidStdLifetime>();
        lifetime.RequestShutdown();
        lifetime.RequestShutdown();

        Assert.True(lifetime.ShutdownToken.IsCancellationRequested);
    }

    [Fact]
    public async Task RunAsync_CallerToken_StillWorks()
    {
        using var temp = new TempDirectory();
        using var cts = new CancellationTokenSource();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path }
        );
        bootstrap.ConfigureServices(c => c.RegisterCoreServices());

        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var bus = bootstrap.Container.Resolve<IEventBus>();
        using var startedSubscription = bus.Subscribe<EngineStartedEvent>(
            (_, _) =>
            {
                started.TrySetResult();

                return Task.CompletedTask;
            }
        );

        var runTask = bootstrap.RunAsync(cts.Token);

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await cts.CancelAsync();
        await runTask.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.True(runTask.IsCompletedSuccessfully);
    }
}
