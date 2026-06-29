# Choosing a storage backend

The storage abstraction (`IStorageService`) stores blobs by key. There are two
providers; the S3 provider also targets any S3-compatible server such as MinIO.

| Backend | Package · entrypoint | Use case | Shared across hosts | Ops cost |
|---|---|---|---|---|
| Local file | `SquidStd.Storage` · `AddFileStorage` | Dev, single host, simple persistence | No | None |
| S3 | `SquidStd.Storage.S3` · `AddS3Storage` | AWS-native object storage | Yes | Managed (AWS) |
| MinIO | `SquidStd.Storage.S3` · `AddS3Storage` (set `ServiceUrl`) | Self-hosted S3-compatible storage | Yes | Run MinIO |

```csharp
bootstrap.ConfigureServices(container => container.AddS3Storage(new S3StorageOptions
{
    // ServiceUrl points at AWS S3 or a self-hosted MinIO endpoint
}));
```

## Recommendation

Use `AddFileStorage` for development and single-host apps. Use `AddS3Storage`
against AWS S3 in the cloud, or against a MinIO endpoint (via `ServiceUrl`) when
you need S3 semantics on self-hosted infrastructure.
