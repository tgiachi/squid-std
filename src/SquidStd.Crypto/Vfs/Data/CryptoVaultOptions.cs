namespace SquidStd.Crypto.Vfs.Data;

/// <summary>Tunables for an encrypted vault: chunk size and Argon2id key-derivation cost parameters.</summary>
public sealed class CryptoVaultOptions
{
    /// <summary>Plaintext chunk size for per-entry AES-GCM, in bytes.</summary>
    public int ChunkSize { get; init; } = 64 * 1024;

    /// <summary>Argon2id memory cost in KiB.</summary>
    public int Argon2MemoryKib { get; init; } = 65536;

    /// <summary>Argon2id iteration (time) cost.</summary>
    public int Argon2Iterations { get; init; } = 3;

    /// <summary>Argon2id parallelism (lanes).</summary>
    public int Argon2Parallelism { get; init; } = 1;
}
