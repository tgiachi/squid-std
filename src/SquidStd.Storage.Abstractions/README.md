<h1 align="center">SquidStd.Storage.Abstractions</h1>

Backend-agnostic storage contracts for SquidStd: a binary blob store (`IStorageService`), a typed object
store (`IObjectStorageService`), key enumeration (`ListKeysAsync`), and `StorageConfig`. Pick a backend
implementation (local file or S3/MinIO) from a companion package.

## Install

```bash
dotnet add package SquidStd.Storage.Abstractions
```

## Usage

```csharp
using SquidStd.Storage.Abstractions.Interfaces;

// Resolve IStorageService from a backend package (file or S3).
public async Task DumpKeysAsync(IStorageService storage)
{
    await foreach (var key in storage.ListKeysAsync("profiles/"))
    {
        Console.WriteLine(key);
    }
}
```

## Key types

| Type                    | Purpose                                                                                         |
|-------------------------|-------------------------------------------------------------------------------------------------|
| `IStorageService`       | Binary blob store: `SaveAsync` / `LoadAsync` / `DeleteAsync` / `ExistsAsync` / `ListKeysAsync`. |
| `IObjectStorageService` | Typed object store over a blob backend (serialized by the provider).                            |
| `StorageConfig`         | Root directory for file storage.                                                                |

## Related

- Tutorial: [Object storage](https://tgiachi.github.io/squid-std/tutorials/storage.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
