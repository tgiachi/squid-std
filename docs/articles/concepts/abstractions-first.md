# Abstractions first

SquidStd is built abstractions-first: code depends on contracts, and concrete backends are chosen at composition time. This is what makes a SquidStd application easy to test and easy to retarget.

## Contract package plus provider packages

Each capability is a contract package paired with one or more provider packages. The contract package — `*.Abstractions` — defines the interfaces and DTOs. Provider packages implement them. Your application references the abstraction and depends on the provider only in the host project. See the [architecture](architecture.md) overview for how this shapes the package graph.

## In-memory for tests, external for prod

Most capabilities ship an in-memory provider for tests and an external-backend provider for production:

- **Messaging** — in-memory, or `SquidStd.Messaging.RabbitMq` / `SquidStd.Messaging.Sqs`.
- **Caching** — in-memory, or `SquidStd.Caching.Redis`.
- **Storage** — file-backed, or `SquidStd.Storage.S3` (S3 and MinIO).

Tests run fast and deterministically against the in-memory provider; production swaps in the external backend.

## Swapping providers without touching call sites

Because call sites depend only on the contract, switching backends is a registration change in the host — `container.AddInMemoryMessaging()` versus `container.AddRabbitMqMessaging(...)` — with no edits to the code that publishes or consumes messages. See [dependency injection](dependency-injection.md) for the registration pattern that makes the swap a one-line change.
