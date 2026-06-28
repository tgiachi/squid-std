<h1 align="center">SquidStd.Persistence.MessagePack</h1>

MessagePack-backed binary `IDataSerializer`/`IDataDeserializer` for `SquidStd.Persistence`. The recommended
compact binary default for entity payloads. Uses the contractless resolver, so plain POCOs need no
attributes.

## Install

```bash
dotnet add package SquidStd.Persistence.MessagePack
```

## Usage

```csharp
using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Persistence.MessagePack;

IDataSerializer serializer = new MessagePackDataSerializer();
IDataDeserializer deserializer = (MessagePackDataSerializer)serializer;

// Pass to a PersistenceEntityDescriptor, or register in DI:
container.RegisterInstance<IDataSerializer>(new MessagePackDataSerializer());
container.RegisterInstance<IDataDeserializer>(new MessagePackDataSerializer());
```

> **Note:** the MessagePack contractless resolver requires **public** entity types. For non-public types,
> use the JSON serializer from `SquidStd.Core` (`JsonDataSerializer`) instead.

## Key types

| Type                       | Purpose                                                  |
|----------------------------|----------------------------------------------------------|
| `MessagePackDataSerializer`| Contractless MessagePack `IDataSerializer`/`IDataDeserializer`. |

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
