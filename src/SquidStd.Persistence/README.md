<h1 align="center">SquidStd.Persistence</h1>

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

## Usage (standalone, no bootstrap)

Wire the stack by hand when you are not using `SquidStdBootstrap` - construct the registry, journal,
snapshot service, and `PersistenceService` yourself:

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

### Manual DI registration (without `RegisterPersistence`)

```csharp
container.RegisterPersistedEntity<Player, int>(typeId: 1, typeName: "Player", schemaVersion: 1, p => p.Id);
container.ApplyPersistedEntityRegistrations();   // builds descriptors into IPersistenceEntityRegistry
```

Only needed when the rest of the stack (registry, journal, snapshot, lifecycle service) is assembled by
hand instead of through `RegisterPersistence()`, which applies these registrations itself - do not call
`ApplyPersistedEntityRegistrations()` when `RegisterPersistence()` is in use.

## Bootstrap registration

The one-call path for `SquidStdBootstrap` apps: register a serializer, register the persistence stack,
then declare the persisted entities.

```csharp
using SquidStd.Persistence.Extensions;
using SquidStd.Persistence.MessagePack.Extensions;

bootstrap.ConfigureServices(c =>
{
    c.RegisterMessagePackSerializer();   // or RegisterDataSerializer() for JSON
    c.RegisterPersistence();             // or RegisterPersistence(new PersistenceConfig { ... })
    c.RegisterPersistedEntity<Player, int>(1, "Player", 1, p => p.Id);
    return c;
});

// after StartAsync: snapshot loaded, journal replayed, autosave running
var players = bootstrap.Resolve<IPersistenceService>().GetStore<Player, int>();
```

- **Serializer prerequisite**: register a serializer (`RegisterMessagePackSerializer()` or
  `RegisterDataSerializer()`) before `RegisterPersistence()` - it throws `InvalidOperationException`
  otherwise, so a missing serializer fails fast instead of at first use.
- **Config source**: `RegisterPersistence()` binds the `persistence` YAML section by default; pass an
  explicit `PersistenceConfig` instance to skip the file entirely for that section (it is then ignored).
  Either way, `SaveDirectory` defaults to the managed `save` directory under the bootstrap root when left
  blank.
- **Lifecycle**: `IPersistenceService` is registered as a lifecycle service - the snapshot loads and the
  journal replays at start, autosave runs while the bootstrap is up, and a final snapshot is written at
  stop.

## Key types

| Type                                  | Purpose                                                      |
|---------------------------------------|--------------------------------------------------------------|
| `PersistenceService`                  | Lifecycle: load + replay, autosave, `GetStore<T,TKey>()`.    |
| `IEntityStore<TEntity,TKey>`          | In-memory CRUD; reads clone, writes journal.                 |
| `PersistenceEntityDescriptor<T,TKey>` | Serializer-injected descriptor (serialize/clone/key).        |
| `PersistenceEntityRegistry`           | Maps `typeId` ↔ descriptor; freezes after registration.      |
| `BinaryJournalService`                | Append-only framed binary WAL with tail-corruption recovery. |
| `SnapshotService`                     | Atomic per-type binary snapshot files with payload checksum. |
| `RegisterPersistedEntity<T,TKey>()`   | DI helper recording an entity for descriptor construction.   |

### Durability

`PersistenceConfig.DurabilityMode` selects how writes reach disk. `Buffered` (default) flushes to the OS
cache - fast, and safe across a process crash. `Durable` fsyncs each journal append and the snapshot temp
file before its atomic rename, so committed data survives power loss. Pass it through when constructing the
services: `new BinaryJournalService(path, config.DurabilityMode)` and
`new SnapshotService(dir, suffix, config.DurabilityMode)`. (.NET has no portable directory fsync, so the
guarantee is per-file content durability plus atomic rename.)

## Related

- Tutorial: [Persistence](https://tgiachi.github.io/squid-std/tutorials/persistence.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
