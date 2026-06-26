# SquidStd.Generators

`SquidStd.Generators` contains Roslyn source generators that create compile-time registration helpers for SquidStd projects.

The first generator discovers concrete `IEventListener<TEvent>` implementations marked with `[RegisterEventListener]` in the consuming project and emits:

```csharp
container.RegisterGeneratedEventListeners();
```

The generated method uses the same runtime path as manual registration:

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

## Supported listener shape

The generator supports public or internal non-generic classes marked with `[RegisterEventListener]` that implement `IEventListener<TEvent>` where `TEvent` is public or internal.

Private nested listener or event types are ignored and reported with diagnostic `SQDGEN001` because generated source cannot reference them safely.
