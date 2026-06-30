using SquidStd.Actors.Extensions;
using SquidStd.Services.Core.Services;
using SquidStd.Tests.Actors.Support;

namespace SquidStd.Tests.Actors;

public class ActorEventBusTests
{
    [Fact]
    public async Task SubscribeToEventBus_MapsEventsIntoMailboxInOrder()
    {
        using var bus = new EventBusService();
        await using var actor = new ProbeActor();
        using var subscription = actor.SubscribeToEventBus(bus, (PingEvent e) => new Append(e.Text));

        await bus.PublishAsync(new PingEvent("x"));
        await bus.PublishAsync(new PingEvent("y"));

        var log = await actor.AskAsync<GetLog, string>(new());
        Assert.Equal("x,y", log);
    }

    [Fact]
    public async Task SubscribeToEventBus_DisposingSubscription_StopsDelivery()
    {
        using var bus = new EventBusService();
        await using var actor = new ProbeActor();
        var subscription = actor.SubscribeToEventBus(bus, (PingEvent e) => new Append(e.Text));

        await bus.PublishAsync(new PingEvent("first"));
        subscription.Dispose();
        await bus.PublishAsync(new PingEvent("second"));

        var log = await actor.AskAsync<GetLog, string>(new());
        Assert.Equal("first", log);
    }
}
