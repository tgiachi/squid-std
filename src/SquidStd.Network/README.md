<h1 align="center">SquidStd.Network</h1>

Networking primitives for SquidStd: TCP and UDP servers and clients with per-connection sessions, a
pluggable framing + middleware pipeline, span-based binary readers/writers, and a circular buffer —
designed for low-allocation, high-throughput byte processing.

## Install

```bash
dotnet add package SquidStd.Network
```

## Usage

```csharp
using System.Net;
using SquidStd.Network.Server;

await using var server = new SquidTcpServer(new IPEndPoint(IPAddress.Any, 9000));
await server.StartAsync(CancellationToken.None);
// ... server.IsRunning, server.Port ...
await server.StopAsync(CancellationToken.None);
```

## Key types

| Type                        | Purpose                                             |
|-----------------------------|-----------------------------------------------------|
| `INetworkServer`            | TCP/UDP server contract (`StartAsync`/`StopAsync`). |
| `INetworkConnection`        | A single client connection.                         |
| `ISessionManager<TState>`   | Tracks sessions and their typed state.              |
| `INetFramer`                | Splits the byte stream into messages.               |
| `INetMiddleware`            | Pipeline stage over inbound/outbound data.          |
| `SpanReader` / `SpanWriter` | Allocation-free binary read/write.                  |

## Related

- Tutorial: [Networking](https://tgiachi.github.io/squid-std/tutorials/networking.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
