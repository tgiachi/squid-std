<h1 align="center">SquidStd.Workers.Manager</h1>

The SquidStd worker manager. Enqueues jobs, collects worker heartbeats into an in-memory registry, marks
workers `Offline` on a periodic sweep, publishes status-transition events, and exposes an opt-in ASP.NET
minimal-API surface.

## Install

```bash
dotnet add package SquidStd.Workers.Manager
```

## Usage

```csharp
using DryIoc;
using SquidStd.Workers.Manager.Extensions;

container.AddInMemoryMessaging(); // or AddRabbitMqMessaging(...)
container.AddWorkerManager();

// In an ASP.NET app, map the opt-in HTTP surface:
app.MapWorkerManagerEndpoints(); // GET /workers, GET /workers/{id}, POST /jobs
```

## Key types

| Type                                  | Purpose                                                             |
|---------------------------------------|---------------------------------------------------------------------|
| `IJobScheduler`                       | `EnqueueAsync(jobName, parameters)` onto the jobs queue.            |
| `IWorkerRegistry`                     | `GetAll()` / `Get(id)` over the live worker view.                   |
| `WorkerStatusChangedEvent`            | Published on the event bus on discover / offline / return.          |
| `WorkerManagerConfig`                 | `OfflineTimeoutSeconds`, `SweepIntervalSeconds`, queue/topic names. |
| `WorkerManagerRegistrationExtensions` | `AddWorkerManager()`.                                               |
| `WorkerManagerEndpointsExtensions`    | `MapWorkerManagerEndpoints()`.                                      |

## Related

- Tutorial: [Worker system](https://tgiachi.github.io/squid-std/tutorials/worker-system.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
