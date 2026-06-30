<h1 align="center">SquidStd.Storage</h1>

Local file storage for SquidStd. Provides a filesystem-backed `IStorageService` (atomic writes, path-safe
keys) and a YAML-backed `IObjectStorageService` that layers typed objects on top of it — registered with a
single `AddFileStorage()` call. Storage is opt-in: it is not registered by `RegisterCoreServices`.

## Install

```bash
dotnet add package SquidStd.Storage
```

## Usage

```csharp
using DryIoc;
using SquidStd.Storage.Abstractions.Data.Config;
using SquidStd.Storage.Abstractions.Interfaces;
using SquidStd.Storage.Extensions;

var container = new Container();
container.AddFileStorage(new StorageConfig { RootDirectory = "data" });

var storage = container.Resolve<IStorageService>();
await storage.SaveAsync("profiles/main.bin", new byte[] { 1, 2, 3 });
```

Saves are atomic (temp file + move) and keys are constrained to the storage root; `ListKeysAsync(prefix?)`
enumerates stored keys (`/`-separated), excluding in-flight temp files.

## Key types

| Type                            | Purpose                                                                                     |
|---------------------------------|---------------------------------------------------------------------------------------------|
| `StorageRegistrationExtensions` | `AddFileStorage(...)` registration (file `IStorageService` + YAML `IObjectStorageService`). |
| `FileStorageService`            | Filesystem-backed `IStorageService`.                                                        |
| `YamlObjectStorageService`      | YAML-backed `IObjectStorageService` over a blob store.                                      |

## Related

- Tutorial: [Object storage](https://tgiachi.github.io/squid-std/tutorials/storage.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
