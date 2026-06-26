<p align="center">
  <img src="https://raw.githubusercontent.com/tgiachi/squid-std/main/assets/icon.png" alt="SquidStd" width="120" height="120" />
</p>

<h1 align="center">SquidStd.Persistence.MessagePack</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/SquidStd.Persistence.MessagePack/"><img src="https://img.shields.io/nuget/v/SquidStd.Persistence.MessagePack.svg" alt="NuGet" /></a>
  <img src="https://img.shields.io/nuget/dt/SquidStd.Persistence.MessagePack.svg" alt="Downloads" />
  <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="license" />
</p>

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
