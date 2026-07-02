# Events, jobs and scheduling

Publish in-process events, run work on a thread pool, and schedule recurring jobs with cron.

## What you'll build

A host that uses three core services from `SquidStd.Services.Core`: the `IEventBus` (publish/subscribe), the
`IJobSystem` (thread-pool work), and the `ICronScheduler` (cron-based recurring jobs).

## Prerequisites

- .NET 10 SDK
- `dotnet add package SquidStd.Services.Core`

The core services are explicit: enable the event bus, job system and timer wheel with
`RegisterCoreServices()`, and add the cron scheduler on top with `RegisterSchedulerServices()`:

[!code-csharp[](../../samples/SquidStd.Samples.EventsJobsScheduling/Program.cs#step-1)]

## Steps

### 1. Publish and handle an event

Implement `IAsyncEventListener<T>`, register it, and publish an `IEvent`. Every registered listener is awaited.

[!code-csharp[](../../samples/SquidStd.Samples.EventsJobsScheduling/Program.cs#step-1)]

### 2. Run work on the job system

`IJobSystem.ScheduleAsync` runs an `Action` on a worker thread and returns a `Task` that completes when it finishes.

[!code-csharp[](../../samples/SquidStd.Samples.EventsJobsScheduling/Program.cs#step-2)]

### 3. Schedule a cron job

`ICronScheduler.Schedule` takes a name, a 5-field cron expression (UTC), and an async handler invoked on each
occurrence.

[!code-csharp[](../../samples/SquidStd.Samples.EventsJobsScheduling/Program.cs#step-3)]

## Run it

```bash
dotnet run --project samples/SquidStd.Samples.EventsJobsScheduling
```

You'll see `received: hello`, `job ran on a worker thread`, and, if you let it run across a 5-minute boundary,
`cron tick`.

## How it works

The event bus and the job system come from `RegisterCoreServices()` - the bus dispatches to sync and async
listeners, the job system is a fixed-size worker-thread pool. The cron scheduler is driven by the timer wheel
(registered by `RegisterSchedulerServices`).

The timer wheel is advanced by a *driver*. `RegisterSchedulerServices()` uses `TimerWheelPumpService`, a
periodic background pump. Apps that need a frame-rate loop can instead call `RegisterEventLoop()`, which advances
the wheel and drains the main-thread dispatcher on a dedicated thread - see
[Scheduler → Event loop](../articles/scheduler.md#event-loop). The two are mutually exclusive: register exactly one.

## See also

- [SquidStd.Services.Core reference](../articles/services-core.md)
- [SquidStd.Core reference](../articles/core.md)
- Previous: [Getting started](getting-started.md)
