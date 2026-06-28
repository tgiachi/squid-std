# Observability

`SquidStd.Telemetry.OpenTelemetry` wires OpenTelemetry tracing and metrics into
the bootstrap as a managed service. Spans flow from SquidStd's own
`ActivitySource` instances and from anything else in your process.

## Steps

1. **Add the package** `SquidStd.Telemetry.OpenTelemetry`.
2. **Register telemetry** with `AddSquidStdTelemetry`, passing a `TelemetryOptions`
   with your `ServiceName`.
3. **Choose an exporter.** Set `OtlpEndpoint` / `OtlpProtocol` to ship to a
   collector (the default endpoint is `http://localhost:4317`, gRPC). For local
   debugging set `EnableConsoleExporter = true` to also print spans to stdout.
4. **Tune sampling** with `TracingSampleRatio` (0..1) and toggle pipelines with
   `EnableTracing` / `EnableMetrics`.

```csharp
bootstrap.ConfigureServices(container =>
    container.AddSquidStdTelemetry(new TelemetryOptions
    {
        ServiceName = "orders-worker",
        OtlpEndpoint = "http://otel-collector:4317",
        OtlpProtocol = OtlpProtocolType.Grpc,
        EnableConsoleExporter = false,
        TracingSampleRatio = 1.0
    }));
```

## ActivitySource convention

SquidStd modules name their `ActivitySource` instances with the `SquidStd.*`
prefix (for example `SquidStd.Messaging`). Subscribe to that prefix in your
collector or tracer-provider configuration to capture all framework spans, and
follow the same convention for your own application sources.
