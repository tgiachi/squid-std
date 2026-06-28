# Choosing a messaging backend

SquidStd exposes one messaging abstraction with three interchangeable backends.
Pick by where you run and what delivery guarantees you need.

| Backend | Package · entrypoint | Use case | Ordering | Durability | Ops cost |
|---|---|---|---|---|---|
| In-memory | `SquidStd.Messaging` · `AddInMemoryMessaging` | Tests, single-process apps | Per-process | None (lost on restart) | None |
| RabbitMQ | `SquidStd.Messaging.RabbitMq` · `AddRabbitMqMessaging` | Self-hosted services, work queues | Per-queue | Durable queues | Run a broker |
| SQS/SNS | `SquidStd.Messaging.Sqs` · `AddSqsMessaging` | AWS-native, serverless | FIFO queues only | Managed, durable | Managed (AWS) |

```csharp
bootstrap.ConfigureServices(container => container.AddRabbitMqMessaging("amqp://localhost"));
```

## Recommendation

Use `AddInMemoryMessaging` for tests and single-process apps, `AddRabbitMqMessaging`
when you self-host, and `AddSqsMessaging` when you are already on AWS.
