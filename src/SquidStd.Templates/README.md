<h1 align="center">SquidStd.Templates</h1>

`dotnet new` templates for scaffolding SquidStd projects. Each template ships a runnable project - a
`Program.cs` wired to `SquidStdBootstrap`, a `squidstd.yaml` config file, and (for the service templates)
a `Dockerfile`. The generated projects reference the SquidStd packages at the same version as the
template pack, so a scaffolded app always lines up with the libraries it targets.

## Install

```bash
dotnet new install SquidStd.Templates
```

List the installed templates with `dotnet new list squidstd`.

## Templates

| Template            | Short name            | What you get                                                    |
|---------------------|-----------------------|-----------------------------------------------------------------|
| Console host        | `squidstd-host`       | A `SquidStdBootstrap` console host with `RegisterCoreServices`. |
| ASP.NET minimal API | `squidstd-aspnetcore` | `UseSquidStd` + health checks + a sample endpoint + Dockerfile. |
| Worker microservice | `squidstd-worker`     | `AddWorkers` + a sample `IJobHandler` + Dockerfile.             |
| Worker manager      | `squidstd-manager`    | `AddWorkerManager` + `MapWorkerManagerEndpoints` + Dockerfile.  |

## Usage

```bash
dotnet new squidstd-host -n Acme.Host
dotnet new squidstd-aspnetcore -n Acme.Api
dotnet new squidstd-worker -n Acme.Worker
dotnet new squidstd-manager -n Acme.Manager
```

Every generated project builds and runs out of the box:

```bash
cd Acme.Worker
dotnet run
```

The console host, for example, scaffolds the standard bootstrap pattern:

```csharp
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(options =>
{
    options.ConfigName = "squidstd";
});

bootstrap.ConfigureServices(container => container.RegisterCoreServices());

await bootstrap.RunAsync();
```

## Template options

| Option          | Templates            | Values                          | Default    |
|-----------------|----------------------|---------------------------------|------------|
| `--messaging`   | worker, manager      | `rabbitmq` \| `inmemory`        | `rabbitmq` |
| `--skipRestore` | all                  | `true` \| `false`               | `false`    |

`--messaging` picks the transport the worker and manager templates wire up: `rabbitmq` for real
multi-process deployments, `inmemory` for trying it out in a single process.

```bash
dotnet new squidstd-worker -n Acme.Resizer --messaging inmemory
dotnet new squidstd-manager -n Acme.Manager --messaging rabbitmq
```

`--skipRestore` skips the automatic `dotnet restore` post-action after creation.

## Related

- Tutorial: [Scaffolding projects](https://tgiachi.github.io/squid-std/tutorials/scaffolding.html)
- Article: [SquidStd.Workers](https://tgiachi.github.io/squid-std/articles/workers.html)
- Article: [SquidStd.Workers.Manager](https://tgiachi.github.io/squid-std/articles/workers-manager.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
