# SquidStd.Generators

Roslyn source generators for SquidStd.

The first generator discovers concrete `IEventListener<TEvent>` implementations marked with `[RegisterEventListener]` in the consuming project and generates a DryIoc registration extension:

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
