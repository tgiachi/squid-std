<h1 align="center">SquidStd.Workers</h1>

The SquidStd worker runtime. `WorkerConsumerService` consumes `JobRequest`s from the jobs queue and
dispatches each to the `IJobHandler` registered for its job name (up to `MaxConcurrency` in parallel),
while `WorkerHeartbeatService` publishes a `WorkerHeartbeat` on the heartbeat topic every few seconds so
a manager can track the fleet. Use it to build worker microservices that pull jobs off a shared queue;
pair it with `SquidStd.Workers.Manager` on the producing side to enqueue jobs and watch heartbeats.

The runtime rides on SquidStd messaging, so a transport must be registered alongside it - in-memory for
a single process, RabbitMQ for real multi-process deployments.

## Install

```bash
dotnet add package SquidStd.Workers
```

## Usage

Register a messaging transport, the worker runtime, and your handlers on the bootstrap container:

```csharp
using SquidStd.Generators.Workers;
using SquidStd.Messaging.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Workers.Extensions;

var bootstrap = SquidStdBootstrap.Create(o => o.ConfigName = "myworker");

bootstrap.ConfigureServices(c => c
    .RegisterCoreServices()
    .AddInMemoryMessaging()          // or AddRabbitMqMessaging(...)
    .AddWorkers()
    .RegisterGeneratedJobHandlers()); // source-generated [RegisterJobHandler] registrations

await bootstrap.StartAsync();
```

A handler implements `IJobHandler`: it names the job it processes and does the work in `HandleAsync`,
reading string parameters off the `JobRequest`. `[RegisterJobHandler]` opts it into generated
registration (or register manually with `AddJobHandler<T>()`):

```csharp
using SquidStd.Workers.Abstractions.Data;
using SquidStd.Workers.Attributes;
using SquidStd.Workers.Interfaces;

[RegisterJobHandler]
public sealed class ResizeImageHandler : IJobHandler
{
    public string JobName => "resize-image";

    public Task HandleAsync(JobRequest job, CancellationToken cancellationToken)
    {
        var width = job.Parameters["width"];
        // ... do the work ...
        return Task.CompletedTask;
    }
}
```

On the producing side, `SquidStd.Workers.Manager`'s `IJobScheduler` (registered with
`AddWorkerManager()`) publishes the `JobRequest` onto the same queue:

```csharp
var scheduler = bootstrap.Resolve<IJobScheduler>();
await scheduler.EnqueueAsync("resize-image", new Dictionary<string, string> { ["width"] = "640" });
```

An unmatched job name raises `JobHandlerNotFoundException` in the dispatcher.

## Configuration

`AddWorkers()` binds the `workers` section of the YAML config to `WorkersConfig`:

```yaml
workers:
  WorkerId: worker-1
  HeartbeatIntervalSeconds: 10
  MaxConcurrency: 4
  JobQueue: squidstd.workers.jobs
  HeartbeatTopic: squidstd.workers.heartbeat
```

| Property                   | Default                       | Notes                                                            |
|----------------------------|-------------------------------|------------------------------------------------------------------|
| `WorkerId`                 | empty                         | Stable identity; blank falls back to the machine/container name. |
| `HeartbeatIntervalSeconds` | `10`                          | Seconds between heartbeats; non-positive falls back to 10.       |
| `MaxConcurrency`           | `0`                           | Parallel jobs; non-positive falls back to the processor count.   |
| `JobQueue`                 | `squidstd.workers.jobs`       | Queue consumed for jobs (`WorkerChannels.JobQueue`).             |
| `HeartbeatTopic`           | `squidstd.workers.heartbeat`  | Topic heartbeats are published to (`WorkerChannels.HeartbeatTopic`). |

The channel defaults come from `WorkerChannels` in `SquidStd.Workers.Abstractions`, shared with the
manager so both sides agree out of the box.

## Key types

| Type                            | Purpose                                                                       |
|---------------------------------|-------------------------------------------------------------------------------|
| `IJobHandler`                   | Handles jobs of one named kind: `JobName` + `HandleAsync(JobRequest, ct)`.   |
| `IJobDispatcher`                | Routes a `JobRequest` to the handler registered for its job name.            |
| `RegisterJobHandlerAttribute`   | Marks handlers for source-generated registration.                            |
| `WorkerConsumerService`         | Lifecycle service that consumes the jobs queue and dispatches.               |
| `WorkerHeartbeatService`        | Lifecycle service that publishes periodic `WorkerHeartbeat`s.                |
| `WorkersConfig`                 | The `workers` config section (identity, heartbeat, concurrency, channels).   |
| `WorkersRegistrationExtensions` | `AddWorkers()` and `AddJobHandler<T>()`.                                     |

## Related

- Tutorial: [Build a worker system](https://tgiachi.github.io/squid-std/tutorials/worker-system.html)
- Article: [SquidStd.Workers.Manager](https://tgiachi.github.io/squid-std/articles/workers-manager.html)
- Article: [SquidStd.Workers.Abstractions](https://tgiachi.github.io/squid-std/articles/workers-abstractions.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
