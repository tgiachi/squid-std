using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

[Collection(SerilogEventSinkCollection.Name)]
public class EngineEventsTests
{
    [Fact]
    public async Task StartAsync_PublishesStartingThenStarted()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path, AppName = "EngineApp" }
        );
        bootstrap.ConfigureServices(c => c.RegisterCoreServices());

        var received = new List<string>();
        EngineStartedEvent? startedEvent = null;
        var bus = bootstrap.Container.Resolve<IEventBus>();
        using var startingSubscription = bus.Subscribe<EngineStartingEvent>(
            (e, _) =>
            {
                received.Add("starting:" + e.Application);

                return Task.CompletedTask;
            }
        );
        using var startedSubscription = bus.Subscribe<EngineStartedEvent>(
            (e, _) =>
            {
                received.Add("started");
                startedEvent = e;

                return Task.CompletedTask;
            }
        );

        try
        {
            await bootstrap.StartAsync();

            Assert.Equal("starting:EngineApp", received[0]);
            Assert.Equal("started", received[1]);
            Assert.NotNull(startedEvent);
            Assert.Equal("EngineApp", startedEvent.Application);
            Assert.True(startedEvent.ServiceCount > 0);
            Assert.True(startedEvent.ElapsedMs >= 0);
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public async Task StopAsync_PublishesStopped()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path, AppName = "EngineApp" }
        );
        bootstrap.ConfigureServices(c => c.RegisterCoreServices());

        EngineStoppedEvent? stoppedEvent = null;
        var bus = bootstrap.Container.Resolve<IEventBus>();
        using var stoppedSubscription = bus.Subscribe<EngineStoppedEvent>(
            (e, _) =>
            {
                stoppedEvent = e;

                return Task.CompletedTask;
            }
        );

        await bootstrap.StartAsync();
        await bootstrap.StopAsync();

        Assert.NotNull(stoppedEvent);
        Assert.Equal("EngineApp", stoppedEvent.Application);
    }

    [Fact]
    public async Task StartStop_WithoutEventBus_DoesNotThrow()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path, AppName = "EngineApp" }
        );

        await bootstrap.StartAsync();
        await bootstrap.StopAsync();
    }
}
