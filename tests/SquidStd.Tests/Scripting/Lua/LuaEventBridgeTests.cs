using MoonSharp.Interpreter;
using SquidStd.Scripting.Lua.Services;

namespace SquidStd.Tests.Scripting.Lua;

public class LuaEventBridgeTests
{
    [Fact]
    public void Invoke_ConvertsNestedPayloadToLuaTable()
    {
        var script = new Script();
        var bridge = new LuaEventBridge();
        bridge.Attach(script);
        var callback = script.DoString(
                                 """
                                 return function(payload)
                                     return payload.actor.name .. ':' .. payload.values[2]
                                 end
                                 """
                             )
                             .Function;

        var result = bridge.Invoke(
            callback,
            new Dictionary<string, object?>
            {
                ["actor"] = new Dictionary<string, object?> { ["name"] = "squid" },
                ["values"] = new List<object?> { 3, 7 }
            }
        );

        Assert.Equal("squid:7", result.String);
    }

    [Fact]
    public void Invoke_WithoutAttachThrows()
    {
        var script = new Script();
        var callback = script.DoString("return function(payload) return payload.name end").Function;
        var bridge = new LuaEventBridge();

        Assert.Throws<InvalidOperationException>(() => bridge.Invoke(callback, new Dictionary<string, object?>()));
    }

    [Fact]
    public void Publish_InvokesRegisteredCallbacksCaseInsensitively()
    {
        var script = new Script();
        var bridge = new LuaEventBridge();
        bridge.Attach(script);
        var callback = script.DoString(
                                 """
                                 calls = 0
                                 captured = nil
                                 return function(payload)
                                     calls = calls + 1
                                     captured = payload.name
                                 end
                                 """
                             )
                             .Function;

        bridge.Register("Spawned", callback);
        bridge.Publish("spawned", new Dictionary<string, object?> { ["name"] = "slime" });

        Assert.Equal(1, (int)script.Globals.Get("calls").Number);
        Assert.Equal("slime", script.Globals.Get("captured").String);
    }

    [Fact]
    public void Invoke_RoutesThroughMarshaller()
    {
        var script = new Script();
        var invoked = 0;
        var marshaller = new DelegatingMarshaller(call => { invoked++; return call(); });
        var bridge = new LuaEventBridge(marshaller);
        bridge.Attach(script);
        var callback = script.DoString("return function(p) return p.name end").Function;

        var result = bridge.Invoke(callback, new Dictionary<string, object?> { ["name"] = "squid" });

        Assert.Equal(1, invoked);
        Assert.Equal("squid", result.String);
    }

    private sealed class DelegatingMarshaller : SquidStd.Scripting.Lua.Interfaces.Events.ILuaInvokeMarshaller
    {
        private readonly Func<Func<DynValue>, DynValue> _wrap;

        public DelegatingMarshaller(Func<Func<DynValue>, DynValue> wrap)
        {
            _wrap = wrap;
        }

        public DynValue Invoke(Func<DynValue> call)
            => _wrap(call);
    }
}
