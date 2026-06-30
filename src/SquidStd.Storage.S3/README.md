<h1 align="center">SquidStd.Storage.S3</h1>

S3-compatible storage for SquidStd, backed by the MinIO .NET SDK. Implements `IStorageService` against
AWS S3, MinIO, or any S3-compatible endpoint, so the same storage API reads and writes object storage. The
bucket is created lazily on first use. Registered with a single `AddS3Storage(...)` call.

## Install

```bash
dotnet add package SquidStd.Storage.S3
```

## Usage

```csharp
using DryIoc;
using SquidStd.Storage.Abstractions.Interfaces;
using SquidStd.Storage.S3.Data.Config;
using SquidStd.Storage.S3.Extensions;

var container = new Container();
container.AddS3Storage(new S3StorageOptions
{
    Endpoint = "localhost:9000",
    AccessKey = "minioadmin",
    SecretKey = "minioadmin",
    Bucket = "app-data"
});

var storage = container.Resolve<IStorageService>();
await storage.SaveAsync("reports/2026.json", "{}"u8.ToArray());
```

`ListKeysAsync(prefix?)` streams object keys via the S3 list API.

## Key types

| Type                              | Purpose                                                   |
|-----------------------------------|-----------------------------------------------------------|
| `S3StorageRegistrationExtensions` | `AddS3Storage(...)` registration.                         |
| `S3StorageService`                | MinIO-backed `IStorageService` with lazy bucket creation. |
| `S3StorageOptions`                | Endpoint, credentials, bucket, TLS, region.               |

## Related

- Tutorial: [Object storage](https://tgiachi.github.io/squid-std/tutorials/storage.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
