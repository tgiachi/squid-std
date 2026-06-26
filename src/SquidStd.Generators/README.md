# SquidStd.Generators

Roslyn source generators for SquidStd compile-time registration helpers.

## Install

```bash
dotnet add package SquidStd.Generators
```

## Event listeners

The event listener generator discovers concrete `IEventListener<TEvent>` implementations marked with `[RegisterEventListener]` and generates a DryIoc registration extension:

```csharp
using SquidStd.Abstractions.Attributes;
using SquidStd.Core.Interfaces.Events;

[RegisterEventListener]
public sealed class PingListener : IEventListener<PingEvent>
{
    public Task HandleAsync(PingEvent eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

container.RegisterGeneratedEventListeners();
```

The generated method reuses the normal `RegisterEventListener<TEvent,TListener>()` runtime path, so listener activation stays compatible with `SquidStd.Services.Core`.

## Other generated registrations

Each registration family has its own marker attribute and generated extension method:

| Marker attribute | Generated method |
|------------------|------------------|
| `[RegisterStdService(typeof(IMyService), Priority = 10)]` | `RegisterGeneratedStdServices()` |
| `[RegisterConfigSection("workers", Priority = -50)]` | `RegisterGeneratedConfigSections()` |
| `[RegisterJobHandler]` | `RegisterGeneratedJobHandlers()` |
| `[RegisterScriptModule]` with `[ScriptModule("name")]` | `RegisterGeneratedScriptModules()` |

The generated methods call the same runtime APIs as manual registration: `RegisterStdService`, `RegisterConfigSection`, `AddJobHandler`, and `RegisterScriptModule`.
