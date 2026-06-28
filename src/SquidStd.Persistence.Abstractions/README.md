<h1 align="center">SquidStd.Persistence.Abstractions</h1>

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
