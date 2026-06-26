# Generated event listener registration

This tutorial shows how to use `SquidStd.Generators` to register event listeners at compile time.

## Add the generator

```bash
dotnet add package SquidStd.Generators
```

## Create an event and listener

```csharp
using SquidStd.Abstractions.Attributes;
using SquidStd.Core.Interfaces.Events;

public sealed record PingEvent(string Message) : IEvent;

[RegisterEventListener]
public sealed class PingListener : IEventListener<PingEvent>
{
    public Task HandleAsync(PingEvent eventData, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(eventData.Message);

        return Task.CompletedTask;
    }
}
```

## Register generated listeners

```csharp
using SquidStd.Generators.Events;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create()
    .ConfigureServices(container => container.RegisterGeneratedEventListeners());

await bootstrap.StartAsync();
```

At build time the generator emits a method that calls:

```csharp
container.RegisterEventListener<PingEvent, PingListener>();
```

The listener is then subscribed during the normal SquidStd service startup.
