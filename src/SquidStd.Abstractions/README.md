<h1 align="center">SquidStd.Abstractions</h1>

DryIoc-based dependency-injection plumbing for SquidStd. It defines the `ISquidStdService` lifecycle
contract and the container extensions used to register services and configuration sections in a uniform,
discoverable way (tracked through ordered registration lists).

## Install

```bash
dotnet add package SquidStd.Abstractions
```

## Usage

```csharp
using DryIoc;
using SquidStd.Abstractions.Attributes;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Services;

var container = new Container();

container.RegisterStdService<IMyService, MyService>();
container.RegisterConfigSection<MyConfig>("my");
```

## Key types

| Type                             | Purpose                                           |
|----------------------------------|---------------------------------------------------|
| `ISquidStdService`               | Async start/stop lifecycle for managed services.  |
| `RegisterEventListenerAttribute` | Marks event listeners for generated registration. |
| `RegisterStdServiceAttribute`    | Marks services for generated registration.        |
| `RegisterConfigSectionAttribute` | Marks config sections for generated registration. |
| `RegisterStdServiceExtension`    | `RegisterStdService<,>` container extension.      |
| `RegisterConfigSectionExtension` | `RegisterConfigSection<>` container extension.    |
| `ServiceRegistrationData`        | Ordered service registration record.              |

## Related

-
Tutorial: [Source generators: registration](https://tgiachi.github.io/squid-std/tutorials/source-generators-registration.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
