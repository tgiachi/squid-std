<h1 align="center">SquidStd.Persistence.MessagePack</h1>

MessagePack-backed binary `IDataSerializer`/`IDataDeserializer` for SquidStd. `MessagePackDataSerializer`
implements both of SquidStd's serialization contracts over the MessagePack **contractless resolver**, so
plain public POCOs round-trip with no `[MessagePackObject]` attributes and no key annotations. It is the
recommended serializer for `SquidStd.Persistence` journals and snapshots: payloads are a fraction of the
size of JSON and (de)serialization is faster, which matters when every entity write is appended to a
binary journal. `SquidStd.Core` ships a JSON default; pull this package in when you want compact binary
payloads instead.

## Install

```bash
dotnet add package SquidStd.Persistence.MessagePack
```

## Usage

The serializer is a single stateless class - construct it and use it directly. `Serialize<T>` returns a
`ReadOnlyMemory<byte>` and `Deserialize<T>` reads one back:

```csharp
using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Persistence.MessagePack;

public sealed class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

var serializer = new MessagePackDataSerializer();

ReadOnlyMemory<byte> bytes = serializer.Serialize(new Player { Id = 1, Name = "Bob" });
Player back = serializer.Deserialize<Player>(bytes);
```

### With SquidStd.Persistence

`PersistenceEntityDescriptor<T, TKey>` takes the serializer and deserializer explicitly, so you can use
MessagePack for entity payloads without touching the rest of the app:

```csharp
using SquidStd.Persistence.Data;
using SquidStd.Persistence.MessagePack;
using SquidStd.Persistence.Services;

var serializer = new MessagePackDataSerializer();

var registry = new PersistenceEntityRegistry();
registry.Register(new PersistenceEntityDescriptor<Player, int>(
    serializer, serializer, typeId: 1, typeName: "Player", schemaVersion: 1, keySelector: p => p.Id));
```

### As the app-wide default serializer

`RegisterCoreServices()` registers the JSON default via `RegisterDataSerializer()`, which uses
`IfAlreadyRegistered.Keep` - an already-registered serializer wins. So to make MessagePack the app-wide
`IDataSerializer`/`IDataDeserializer` (used by messaging, caching, and DI-registered persistence
descriptors), register it **before** `RegisterCoreServices()`:

```csharp
using DryIoc;
using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Persistence.MessagePack;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(o => o.ConfigName = "myapp");

bootstrap.ConfigureServices(c =>
{
    var serializer = new MessagePackDataSerializer();
    c.RegisterInstance<IDataSerializer>(serializer);
    c.RegisterInstance<IDataDeserializer>(serializer);

    return c.RegisterCoreServices(); // keeps your MessagePack registration
});

await bootstrap.StartAsync();
```

If you register it *after* `RegisterCoreServices()`, the JSON default is already in place and stays -
the order matters, so always register MessagePack first.

## Notes

- **Public types only**: the contractless resolver requires **public** entity types. For non-public
  types, use the JSON serializer from `SquidStd.Core` (`JsonDataSerializer`) instead.
- **Attribute-free**: `ContractlessStandardResolver` serializes by member name, so no
  `[MessagePackObject]`/`[Key]` attributes are needed - but member *names* become part of the payload
  contract, so renaming a property is a schema change (bump `schemaVersion` on persisted entities).
- There is no registration extension - the class is the whole package. Register the same instance for
  both interfaces so serialize and deserialize agree.

## Key types

| Type                        | Purpose                                                          |
|-----------------------------|------------------------------------------------------------------|
| `MessagePackDataSerializer` | Contractless MessagePack `IDataSerializer`/`IDataDeserializer`.  |

## Related

- Article: [Serialization](https://tgiachi.github.io/squid-std/articles/serialization.html)
- Article: [SquidStd.Persistence](https://tgiachi.github.io/squid-std/articles/persistence.html)
- Article: [SquidStd.Persistence.Abstractions](https://tgiachi.github.io/squid-std/articles/persistence-abstractions.html)
- Tutorial: [Durable entity persistence](https://tgiachi.github.io/squid-std/tutorials/persistence.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
