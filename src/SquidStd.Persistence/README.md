<p align="center">
  <img src="https://raw.githubusercontent.com/tgiachi/squid-std/main/assets/icon.png" alt="SquidStd" width="120" height="120" />
</p>

<h1 align="center">SquidStd.Persistence</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/SquidStd.Persistence/"><img src="https://img.shields.io/nuget/v/SquidStd.Persistence.svg" alt="NuGet" /></a>
  <img src="https://img.shields.io/nuget/dt/SquidStd.Persistence.svg" alt="Downloads" />
  <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="license" />
</p>

Embeddable in-memory entity store with durable **binary snapshot + journal (write-ahead log)** persistence.
Full state lives in memory (synchronous reads), every mutation is appended to a length+checksum-framed
binary journal, and a periodic snapshot captures all state and trims the journal. On startup the engine
loads the snapshot and replays the journal tail. The engine is **serializer-agnostic** (via SquidStd's
`IDataSerializer`/`IDataDeserializer`) and has no MessagePack or domain dependency.

## Install

```bash
dotnet add package SquidStd.Persistence
dotnet add package SquidStd.Persistence.MessagePack   # recommended binary serializer
```

## Features

- **Snapshot + journal**: in-memory state, WAL journal of every upsert/remove, periodic full snapshot + trim.
- **Crash-safe**: journal records are length+FNV-1a-checksum framed — a torn/corrupt trailing record is
  detected on read and the tail is discarded. Snapshots are written atomically (temp + rename).
- **Serializer-agnostic**: per-entity payloads go through `IDataSerializer`/`IDataDeserializer`; the journal
  and snapshot envelopes use a fixed binary layout. Pair with `SquidStd.Persistence.MessagePack` for a
  compact binary default, or use the JSON serializer from `SquidStd.Core`.
- **Detached reads**: `GetByIdAsync`/`GetAllAsync`/`Query()` return deep clones, so callers never mutate
  stored instances.
- **Write-ordered journaling**: writes serialize end-to-end (apply + append) so journal order always
  matches sequence order — replay is deterministic.
- **Lifecycle service**: `PersistenceService` is an `ISquidStdService` that loads + replays at start,
  autosaves on a timer, and snapshots on stop. Optional `IEventBus` integration raises
  `SnapshotSaveStartedEvent`/`SnapshotSaveCompletedEvent`.

## Usage

```csharp
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.MessagePack;
using SquidStd.Persistence.Services;

public sealed class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

var serializer = new MessagePackDataSerializer();

var registry = new PersistenceEntityRegistry();
registry.Register(new PersistenceEntityDescriptor<Player, int>(
    serializer, serializer, typeId: 1, typeName: "Player", schemaVersion: 1, keySelector: p => p.Id));

var config = new PersistenceConfig { SaveDirectory = "./save" };
var journal = new BinaryJournalService(Path.Combine(config.SaveDirectory, config.JournalFileName));
var snapshot = new SnapshotService(config.SaveDirectory, config.SnapshotFileSuffix);

var persistence = new PersistenceService(registry, journal, snapshot, config);
await persistence.InitializeAsync();                       // load snapshot + replay journal

var players = persistence.GetStore<Player, int>();
await players.UpsertAsync(new Player { Id = 1, Name = "Bob" });   // appended to the journal
await persistence.SaveSnapshotAsync();                            // snapshot + trim journal

var bob = await players.GetByIdAsync(1);                          // detached clone
```

### DI registration

```csharp
container.RegisterPersistedEntity<Player, int>(typeId: 1, typeName: "Player", schemaVersion: 1, p => p.Id);
container.ApplyPersistedEntityRegistrations();   // builds descriptors into IPersistenceEntityRegistry
```

## Key types

| Type                                  | Purpose                                                        |
|---------------------------------------|---------------------------------------------------------------|
| `PersistenceService`                  | Lifecycle: load + replay, autosave, `GetStore<T,TKey>()`.     |
| `IEntityStore<TEntity,TKey>`          | In-memory CRUD; reads clone, writes journal.                  |
| `PersistenceEntityDescriptor<T,TKey>` | Serializer-injected descriptor (serialize/clone/key).         |
| `PersistenceEntityRegistry`           | Maps `typeId` ↔ descriptor; freezes after registration.       |
| `BinaryJournalService`                | Append-only framed binary WAL with tail-corruption recovery.  |
| `SnapshotService`                     | Atomic per-type binary snapshot files with payload checksum.  |
| `RegisterPersistedEntity<T,TKey>()`   | DI helper recording an entity for descriptor construction.    |

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
