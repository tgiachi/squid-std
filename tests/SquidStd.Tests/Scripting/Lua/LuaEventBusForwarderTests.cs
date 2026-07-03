using MoonSharp.Interpreter;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Data.Events;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Scripting.Lua.Services;
using SquidStd.Services.Core.Services;

namespace SquidStd.Tests.Scripting.Lua;

public class LuaEventBusForwarderTests
{
    [Fact]
    public async Task EngineStartedEvent_ReachesLuaCallback_WithSnakeCaseNameAndPayload()
    {
        using var bus = new EventBusService(new EventBusOptions());
        var script = new Script();
        var bridge = new LuaEventBridge();
        bridge.Attach(script);
        var forwarder = new LuaEventBusForwarder(bridge, bus);
        await forwarder.StartAsync();
        var callback = script.DoString(
                                 """
                                 return function(e)
                                     captured = e.application .. ':' .. e.service_count .. ':' .. tostring(e.elapsed_ms >= 0)
                                 end
                                 """
                             )
                             .Function;
        bridge.Register("engine_started", callback);

        await bus.PublishAsync(new EngineStartedEvent("MyApp", 5, 12.5));

        Assert.Equal("MyApp:5:true", script.Globals.Get("captured").String);

        forwarder.Dispose();
    }

    [Fact]
    public void EventTypeWithoutEventSuffix_UsesFullSnakeName()
    {
        Assert.Equal("custom_signal", LuaEventBusForwarder.DeriveName(typeof(CustomSignal)));
        Assert.Equal("engine_started", LuaEventBusForwarder.DeriveName(typeof(EngineStartedEvent)));
    }

    [Fact]
    public async Task Forwarder_WithoutBus_IsNoOp()
    {
        var bridge = new LuaEventBridge();
        var forwarder = new LuaEventBusForwarder(bridge);

        await forwarder.StartAsync();
        await forwarder.StopAsync();
        forwarder.Dispose();
    }

    [Fact]
    public async Task ConcurrentPublishes_AreSerializedIntoLua()
    {
        using var bus = new EventBusService(new EventBusOptions());
        var script = new Script();
        var bridge = new LuaEventBridge();
        bridge.Attach(script);
        var forwarder = new LuaEventBusForwarder(bridge, bus);
        await forwarder.StartAsync();
        script.Globals["counter"] = 0;
        var callback = script.DoString("return function(e) counter = counter + 1 end").Function;
        bridge.Register("engine_stopped", callback);

        var publishes = Enumerable.Range(0, 200)
                                  .Select(_ => Task.Run(() => bus.PublishAsync(new EngineStoppedEvent("app"))));
        await Task.WhenAll(publishes);

        Assert.Equal(200, (int)script.Globals.Get("counter").Number);

        forwarder.Dispose();
    }

    private sealed record CustomSignal : IEvent;
}
