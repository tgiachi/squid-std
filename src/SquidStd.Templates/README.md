<h1 align="center">SquidStd.Templates</h1>

`dotnet new` templates for scaffolding SquidStd projects. The generated projects reference the SquidStd packages
at the same version as this template pack.

## Install

```bash
dotnet new install SquidStd.Templates
```

## Templates

| Template            | Short name            | What you get                                                    |
|---------------------|-----------------------|-----------------------------------------------------------------|
| Console host        | `squidstd-host`       | A `SquidStdBootstrap` console host.                             |
| ASP.NET minimal API | `squidstd-aspnetcore` | `UseSquidStd` + health checks + a sample endpoint + Dockerfile. |
| Worker microservice | `squidstd-worker`     | `AddWorkers` + a sample `IJobHandler` + Dockerfile.             |
| Worker manager      | `squidstd-manager`    | `AddWorkerManager` + `MapWorkerManagerEndpoints` + Dockerfile.  |

## Usage

```bash
dotnet new squidstd-worker -n Acme.Resizer
dotnet new squidstd-worker -n Acme.Resizer --messaging inmemory
dotnet new squidstd-manager -n Acme.Manager
```

`--messaging` (`rabbitmq` default | `inmemory`) is available on the worker and manager templates.

## Related

- Tutorial: [Scaffolding projects](https://tgiachi.github.io/squid-std/tutorials/scaffolding.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
