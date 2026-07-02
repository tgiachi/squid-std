# Scheduler (Cron)

`ICronScheduler` (in `SquidStd.Core`, implemented in `SquidStd.Services.Core`) runs asynchronous jobs on
standard 5-field cron expressions evaluated in UTC.

- `Schedule(name, cronExpression, handler)` → returns a job id.
- `Unschedule(jobId)` / `UnscheduleByName(name)`.
- `Jobs` — a snapshot of registered jobs (`CronJobInfo`).

Each job is a one-shot, self-rescheduling timer on the timer wheel: when it fires, the handler is
dispatched through `IJobSystem`, and the next occurrence is registered. An occurrence is **skipped** if the
previous run of the same job is still in flight. Because the timer wheel must be advanced, the package also
provides `TimerWheelPumpService`, which pumps the wheel on a background loop.

Register everything (after `RegisterCoreServices`) with `RegisterSchedulerServices()`:

```csharp
using DryIoc;
using SquidStd.Core.Interfaces.Scheduling;
using SquidStd.Services.Core.Extensions;

container.RegisterSchedulerServices();

var scheduler = container.Resolve<ICronScheduler>();
scheduler.Schedule("cleanup", "0 3 * * *", async ct =>
{
    await DoCleanupAsync(ct);
});
```

## Event loop

For applications that need a tight, frame-driven loop (game servers, simulations, real-time
processing), `SquidStd.Services.Core` provides `EventLoopService` — a dedicated background thread
(`SquidStd-EventLoop`) that, every frame:

1. drains the `IMainThreadDispatcher` (deferred callbacks posted with `Post`), and
2. advances the timer wheel (`ITimerService.UpdateTicksDelta`),

then sleeps `IdleSleepMs` (default 1 ms) when a frame produced no work. It exposes tick metrics
(`tick_count`, `tick_avg_ms`, `tick_max_ms`, `idle_sleeps_total`) under the `eventloop` provider and
logs a warning when a tick takes longer than `SlowTickThresholdMs` (default 250 ms).

Register it (after `RegisterCoreServices`) with `RegisterEventLoop()`:

```csharp
using DryIoc;
using SquidStd.Core.Interfaces.Threading;
using SquidStd.Services.Core.Extensions;

container.RegisterEventLoop();

var loop = container.Resolve<IEventLoopService>();
Console.WriteLine($"{loop.TickCount} ticks, avg {loop.AverageTickMs:0.###} ms");
```

Configure it via the `eventLoop` section (section keys are matched as registered, property names are
PascalCase):

```yaml
eventLoop:
  IdleCpuEnabled: true      # sleep when a tick produced no work
  IdleSleepMs: 1            # how long to sleep when idle
  SlowTickThresholdMs: 250  # warn above this per-tick time
```

### Event loop vs. timer-wheel pump

Both `EventLoopService` and `TimerWheelPumpService` advance the timer wheel, so they are **mutually
exclusive** — register exactly one. The exclusivity is structural: both implement the
`ITimerWheelDriver` marker, `RegisterEventLoop()` throws if a driver is already registered, and modules
that need the wheel (the worker manager, the mail poller) auto-register the pump only when no driver is
present. Use the pump for ordinary apps where a coarse periodic pump is enough; use the event loop when
you want the wheel advanced at frame-rate alongside dispatcher draining.
