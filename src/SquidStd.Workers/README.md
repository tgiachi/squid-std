<h1 align="center">SquidStd.Workers</h1>

The SquidStd worker runtime. Consumes `JobRequest`s from the jobs queue, dispatches each to a named
`IJobHandler` (up to `MaxConcurrency` in parallel), and publishes a `WorkerHeartbeat` on the heartbeat topic
every few seconds.

## Install

```bash
dotnet add package SquidStd.Workers
```

## Usage

```csharp
using DryIoc;
using SquidStd.Generators.Workers;
using SquidStd.Workers.Attributes;
using SquidStd.Workers.Extensions;

container.AddInMemoryMessaging(); // or AddRabbitMqMessaging(...)
container.AddWorkers();
container.RegisterGeneratedJobHandlers();
```

```csharp
[RegisterJobHandler]
public sealed class ResizeImageHandler : IJobHandler
{
    public string JobName => "resize-image";

    public Task HandleAsync(JobRequest job, CancellationToken cancellationToken)
    {
        // job.Parameters["width"], ...
        return Task.CompletedTask;
    }
}
```

## Key types

| Type                            | Purpose                                                                      |
|---------------------------------|------------------------------------------------------------------------------|
| `IJobHandler`                   | Handles jobs of one named kind.                                              |
| `WorkersConfig`                 | `WorkerId`, `HeartbeatIntervalSeconds`, `MaxConcurrency`, queue/topic names. |
| `RegisterJobHandlerAttribute`   | Marks handlers for generated registration.                                   |
| `WorkersRegistrationExtensions` | `AddWorkers()` and `AddJobHandler<T>()`.                                     |

## Related

- Tutorial: [Worker system](https://tgiachi.github.io/squid-std/tutorials/worker-system.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
