# Scripting with Lua

Embed a Lua engine in your host, expose C# values to scripts, and read results back.

## What you'll build

A console host that registers the `IScriptEngineService` (`SquidStd.Scripting.Lua`) through the SquidStd
bootstrap, runs a small Lua script, and prints values evaluated by the engine.

## Prerequisites

- .NET 10 SDK
- `dotnet add package SquidStd.Scripting.Lua`
- `dotnet add package SquidStd.Services.Core`
- `dotnet add package SquidStd.Generators`

## Steps

### 1. Register the Lua engine

The engine needs a `LuaEngineConfig` (it watches a scripts directory) and is started by the bootstrap as a
SquidStd service. The `DirectoriesConfig` it depends on is already registered by the core services. The generated
script-module registration call adds any `[RegisterScriptModule]` modules before startup.

[!code-csharp[](../../samples/SquidStd.Samples.ScriptingLua/Program.cs#step-1)]

### 2. Run a script and read results

`RegisterGlobal` exposes a C# value under an exact name, `ExecuteScript` runs Lua code, and
`ExecuteFunction` evaluates an expression and returns a `ScriptResult` whose `Data` holds the value.

[!code-csharp[](../../samples/SquidStd.Samples.ScriptingLua/Program.cs#step-2)]

### 3. Define a generated module

`[RegisterScriptModule]` opts the type into source generation. `[ScriptModule("sample")]` is still the runtime Lua
metadata used as the module name.

[!code-csharp[](../../samples/SquidStd.Samples.ScriptingLua/Program.cs#step-3)]

## Run it

```bash
dotnet run --project samples/SquidStd.Samples.ScriptingLua
```

Prints:

```
lua modules = 3
3 + 4 = 7
result = hello from C# and lua
```

## How it works

`IScriptEngineService` wraps a MoonSharp `Script`. `RegisterLuaEngine` registers the `LuaEngineConfig` instance
and the engine as the standard script engine service in one call, so the bootstrap calls its `StartAsync`
during `StartAsync`, wiring up modules, constants, and a file watcher over the configured scripts directory.
Globals you register become Lua variables, and `ExecuteFunction` evaluates a `return <expression>` and
surfaces the value through `ScriptResult.Data`.

## See also

- [SquidStd.Scripting.Lua reference](../articles/scripting-lua.md)
- [Generated registrations](source-generators-registration.md)
