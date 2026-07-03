<h1 align="center">SquidStd.Scripting.Lua</h1>

Lua scripting for SquidStd. Hosts a Lua engine (`IScriptEngineService`) that exposes .NET methods to
scripts through attribute-decorated modules, bridges events to the SquidStd event bus, ships built-in
modules (logging, events, random), generates `.luarc` typings/docs, and supports init scripts, constants,
and callbacks.

## Install

```bash
dotnet add package SquidStd.Scripting.Lua
```

## Usage

```csharp
using DryIoc;
using SquidStd.Generators.Scripting.Lua;
using SquidStd.Scripting.Lua.Attributes;
using SquidStd.Scripting.Lua.Attributes.Scripts;
using SquidStd.Scripting.Lua.Extensions.Scripts;

[RegisterScriptModule]
[ScriptModule("math2")]
public sealed class MathModule
{
    [ScriptFunction("add")]
    public int Add(int a, int b) => a + b;
}

var container = new Container();
container.RegisterGeneratedScriptModules();
// Resolve IScriptEngineService to load and execute scripts that call math2.add(1, 2).
```

## Subscribing to server events

Register the Lua events stack, then subscribe from any script by snake_case event name - the
CLR type name minus the `Event` suffix (`EngineStartedEvent` becomes `engine_started`). Payload
keys are snake_case too. The convention applies to every event published on the event bus.

```csharp
container.RegisterLuaEvents();   // bridge + events module + bus forwarder
```

```lua
events.subscribe("engine_started", function(e)
    log.info("engine ready: " .. e.application .. " (" .. e.service_count .. " services)")
end)
```

The forwarder is a no-op when no event bus is registered. `events.on` remains available as an
alias of `subscribe`.

## Key types

| Type                                                | Purpose                                                             |
|-----------------------------------------------------|---------------------------------------------------------------------|
| `IScriptEngineService`                              | Lua engine: load/run scripts, register modules/constants/callbacks. |
| `ILuaEventBridge`                                   | Bridges Lua scripts to the event bus.                               |
| `ScriptModuleAttribute` / `ScriptFunctionAttribute` | Expose .NET classes/methods to Lua.                                 |
| `RegisterScriptModuleAttribute`                     | Marks script modules for generated registration.                    |
| `AddScriptModuleExtension`                          | `RegisterScriptModule<T>()` / `RegisterLuaUserData<T>()`.           |

## Related

- Tutorial: [Scripting with Lua](https://tgiachi.github.io/squid-std/tutorials/scripting-lua.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
