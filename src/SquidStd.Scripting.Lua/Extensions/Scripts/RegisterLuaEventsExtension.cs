using DryIoc;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Scripting.Lua.Interfaces.Events;
using SquidStd.Scripting.Lua.Modules;
using SquidStd.Scripting.Lua.Services;

namespace SquidStd.Scripting.Lua.Extensions.Scripts;

/// <summary>
/// Registration helpers for the Lua events stack: bridge, events module and bus forwarder.
/// </summary>
public static class RegisterLuaEventsExtension
{
    /// <param name="container">Container that receives the registrations.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the Lua event bridge, the <c>events</c> script module and the bus forwarder
        /// that delivers every published event to Lua callbacks by snake_case name
        /// (<c>events.subscribe("engine_started", fn)</c>). The forwarder is a no-op when no
        /// event bus is registered.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterLuaEvents()
        {
            container.Register<ILuaInvokeMarshaller, InlineLuaInvokeMarshaller>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);
            container.Register<ILuaEventBridge, LuaEventBridge>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);
            container.RegisterScriptModule<EventsModule>();
            container.RegisterStdService<LuaEventBusForwarder, LuaEventBusForwarder>();

            return container;
        }
    }
}
