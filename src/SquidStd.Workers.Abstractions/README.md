<h1 align="center">SquidStd.Workers.Abstractions</h1>

Shared, dependency-free contracts between the SquidStd worker runtime and manager: the job descriptor, the
worker heartbeat, the manager-side worker view, the worker status enum, and the conventional channel names.

## Install

```bash
dotnet add package SquidStd.Workers.Abstractions
```

## Key types

| Type               | Purpose                                                                            |
|--------------------|------------------------------------------------------------------------------------|
| `JobRequest`       | A job (name + string parameters) enqueued by the manager and consumed by a worker. |
| `WorkerHeartbeat`  | Liveness signal a worker publishes (status, active jobs, max concurrency).         |
| `WorkerInfo`       | The manager-side view of a worker, folded from heartbeats.                         |
| `WorkerStatusType` | `Idle` / `Busy` / `Offline`.                                                       |
| `WorkerChannels`   | Default jobs-queue and heartbeat-topic names.                                      |

## Related

- Tutorial: [Worker system](https://tgiachi.github.io/squid-std/tutorials/worker-system.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
