<p align="center">
  <img src="https://raw.githubusercontent.com/tgiachi/squid-std/main/assets/icon.png" alt="SquidStd" width="120" height="120" />
</p>

<h1 align="center">SquidStd.Persistence.Abstractions</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/SquidStd.Persistence.Abstractions/"><img src="https://img.shields.io/nuget/v/SquidStd.Persistence.Abstractions.svg" alt="NuGet" /></a>
  <img src="https://img.shields.io/nuget/dt/SquidStd.Persistence.Abstractions.svg" alt="Downloads" />
  <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="license" />
</p>

Contracts and DTOs for SquidStd's binary persistence engine. Depend on this package to expose or consume
persistence types without pulling in the engine implementation.

## Install

```bash
dotnet add package SquidStd.Persistence.Abstractions
```

## Key types

| Type                                  | Purpose                                                       |
|---------------------------------------|---------------------------------------------------------------|
| `IPersistenceService`                 | Lifecycle (`ISquidStdService`) + `GetStore<TEntity,TKey>()`.  |
| `IEntityStore<TEntity,TKey>`          | In-memory CRUD contract (clones on read, journals on write).  |
| `IPersistenceEntityRegistry`          | Registry of entity descriptors keyed by `typeId`/type pair.   |
| `IPersistenceEntityDescriptor<T,TKey>`| Serialize / deserialize / clone / key extraction contract.    |
| `ISnapshotService` / `IJournalService`| Per-type snapshot and append-only journal contracts.          |
| `PersistenceConfig`                   | Autosave cadence, file names, save directory, file lock.      |
| `JournalEntry`, `EntitySnapshotBucket`, `PersistedBucket`, `SnapshotFileEnvelope` | Persisted DTOs. |
| `JournalEntityOperationType`          | `Upsert` / `Remove` journal operation discriminator.          |
| `SnapshotSaveStartedEvent` / `SnapshotSaveCompletedEvent` | Snapshot lifecycle events (`IEvent`). |

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
