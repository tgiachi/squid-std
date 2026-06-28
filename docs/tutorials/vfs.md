# Virtual filesystem

Read and write files through an abstract virtual filesystem, then layer an encrypted vault on top.

## What you'll build

A host that resolves `IVirtualFileSystem` (`SquidStd.Vfs`) backed by an in-memory store, plus an
encrypted-vault round-trip using `CryptoFileSystem` (`SquidStd.Crypto.Vfs`).

## Prerequisites

- .NET 10 SDK
- `dotnet add package SquidStd.Vfs` (and `SquidStd.Crypto.Vfs` for the encrypted vault)

## Steps

### 1. Register a virtual filesystem

`RegisterVfs` wires `IVirtualFileSystem`; the factory chooses the backend — here a plain
in-memory filesystem.

[!code-csharp[](../../samples/SquidStd.Samples.Vfs/Program.cs#step-1)]

### 2. Write and read a file

The same `IVirtualFileSystem` API works regardless of the backend. `ReadAllBytesAsync` returns
`null` only when the path is absent.

[!code-csharp[](../../samples/SquidStd.Samples.Vfs/Program.cs#step-2)]

### 3. Encrypted vault round-trip

`CryptoFileSystem` encrypts every entry over a backend filesystem. The lifecycle is
unlock → write → dispose; disposing locks the vault (zeroing the key and flushing the encrypted
index) and disposes the backend, so a `ZipFileSystem` backend flushes its archive to disk.
Re-opening a fresh instance over the same file with the passphrase decrypts the data at rest.

This sample backs the vault with a single on-disk zip file via `ZipFileSystem`, writes a secret,
disposes the vault, then re-opens a brand-new instance over the same file to prove on-disk
persistence. The DI helper `RegisterCryptoVault` wires exactly this — a vault over a single-file
zip — as a lockable singleton.

[!code-csharp[](../../samples/SquidStd.Samples.Vfs/Program.cs#step-3)]

## Run it

```bash
dotnet run --project samples/SquidStd.Samples.Vfs
```

## Next steps

- [SquidStd.Vfs reference](../articles/vfs.md)
