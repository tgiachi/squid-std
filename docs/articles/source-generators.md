# SquidStd.Generators

`SquidStd.Generators` contains Roslyn source generators that create compile-time registration helpers for SquidStd projects.

Generators are opt-in per registration surface. Mark the type you want registered and call the generated extension method while configuring the DryIoc container:

```csharp
container.RegisterGeneratedEventListeners();
container.RegisterGeneratedStdServices();
container.RegisterGeneratedConfigSections();
container.RegisterGeneratedJobHandlers();
container.RegisterGeneratedScriptModules();
```

Each generated method uses the same runtime path as manual registration. For example, the event listener generator emits:

```csharp
container.RegisterEventListener<PingEvent, PingListener>();
```

This keeps startup behavior compatible with `EventListenerActivator` and the normal `SquidStdBootstrap` lifecycle.

## Usage

Add the generator package:

```bash
dotnet add package SquidStd.Generators
```

Call the generated extension while configuring the DryIoc container:

```csharp
using SquidStd.Abstractions.Attributes;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Generators.Events;

[RegisterEventListener]
public sealed class PingListener : IEventListener<PingEvent>
{
    public Task HandleAsync(PingEvent eventData, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(eventData.Message);

        return Task.CompletedTask;
    }
}

bootstrap.ConfigureServices(container => container.RegisterGeneratedEventListeners());
```

## Generated registration families

### Event listeners

Use `[RegisterEventListener]` on a concrete `IEventListener<TEvent>` implementation and call:

```csharp
using SquidStd.Generators.Events;

container.RegisterGeneratedEventListeners();
```

### Standard services

Use `[RegisterStdService(typeof(IMyService), Priority = 10)]` on the implementation and call:

```csharp
using SquidStd.Generators.Services;

container.RegisterGeneratedStdServices();
```

The generator emits `RegisterStdService<IMyService, MyService>(container, 10)`.

### Config sections

Use `[RegisterConfigSection("workers", Priority = -50)]` on the config type and call:

```csharp
using SquidStd.Generators.Config;

container.RegisterGeneratedConfigSections();
```

The generator emits `RegisterConfigSection<WorkersConfig>(container, "workers", priority: -50)`.

### Job handlers

Use `[RegisterJobHandler]` on an `IJobHandler` implementation and call:

```csharp
using SquidStd.Generators.Workers;

container.RegisterGeneratedJobHandlers();
```

The generator emits `AddJobHandler<MyJobHandler>(container)`.

### Lua script modules

Use `[RegisterScriptModule]` together with the runtime `[ScriptModule("name")]` metadata and call:

```csharp
using SquidStd.Generators.Scripting.Lua;

container.RegisterGeneratedScriptModules();
```

The generator emits `RegisterScriptModule<MyScriptModule>(container)`.

## Supported shapes and diagnostics

Generated source can only reference public or internal non-generic concrete classes. Unsupported types are skipped and reported as warnings.

| Diagnostic | Registration family |
|------------|---------------------|
| `SQDGEN001` | Event listener cannot be generated. |
| `SQDGEN002` | Standard service cannot be generated. |
| `SQDGEN003` | Config section cannot be generated. |
| `SQDGEN004` | Job handler cannot be generated. |
| `SQDGEN005` | Script module cannot be generated. |
