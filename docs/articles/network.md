[!include[](../../src/SquidStd.Network/README.md)]

## The receive pipeline

```mermaid
flowchart LR
  Bytes[Raw bytes] --> MW1[Middleware 1]
  MW1 --> MW2[Middleware N]
  MW2 --> Framing[INetFramer<br/>frame extraction]
  Framing --> Handler[Message handler]
  Handler -->|response| Out[Write pipeline]
```

Each connection runs inbound bytes through the middleware chain (`INetMiddleware`), then the configured `INetFramer` splits the accumulated stream into discrete frames before they reach your handler. The write path runs the same middleware chain on outgoing payloads before the bytes hit the socket.
