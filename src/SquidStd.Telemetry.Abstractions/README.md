<h1 align="center">SquidStd.Telemetry.Abstractions</h1>

Dependency-free telemetry configuration shared by SquidStd observability providers
(e.g. `SquidStd.Telemetry.OpenTelemetry`). Holds `TelemetryOptions`, the `OtlpProtocolType` enum, and
the `SquidStdActivity` helper / `SquidStd.*` ActivitySource naming convention. No OpenTelemetry
dependency: custom spans use the BCL `System.Diagnostics.ActivitySource`.

## Install

```bash
dotnet add package SquidStd.Telemetry.Abstractions
```

## Usage

```csharp
using SquidStd.Telemetry.Abstractions;

using var activity = SquidStdActivity.Source.StartActivity("checkout");
activity?.SetTag("order.id", orderId);
```

Subsystems create their own `new ActivitySource("SquidStd.Something")`; the OpenTelemetry provider
captures every `SquidStd.*` source automatically.

## Key types

| Type               | Purpose                                                                    |
|--------------------|----------------------------------------------------------------------------|
| `TelemetryOptions` | Service name, OTLP endpoint/protocol, sampling and exporter configuration. |
| `OtlpProtocolType` | OTLP transport: gRPC or HTTP.                                              |
| `SquidStdActivity` | Shared `ActivitySource` and the `SquidStd.*` source naming convention.     |

## Related

- Article: [Telemetry abstractions](https://tgiachi.github.io/squid-std/articles/telemetry-abstractions.html)
- Guide: [Observability](https://tgiachi.github.io/squid-std/articles/guides/observability.html)
- [`SquidStd.Telemetry.OpenTelemetry`](https://tgiachi.github.io/squid-std/articles/telemetry-opentelemetry.html) - OTLP tracing/metrics provider consuming these options

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
