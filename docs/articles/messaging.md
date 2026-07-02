[!include[](../../src/SquidStd.Messaging/README.md)]

## How messages flow

```mermaid
flowchart LR
  P[Publisher] -->|enqueue| Q[(Queue)]
  Q -->|competing consumers,<br/>one receives| C1[Consumer A]
  Q -.-> C2[Consumer B]

  P2[Publisher] -->|publish| T((Topic))
  T -->|fan-out,<br/>all receive| S1[Subscriber A]
  T --> S2[Subscriber B]
```

Queues deliver each message to exactly one consumer (work distribution); topics fan out every message to all subscribers (notifications). Both are behind `IMessageQueue` / `IMessageTopic` regardless of the backend.
