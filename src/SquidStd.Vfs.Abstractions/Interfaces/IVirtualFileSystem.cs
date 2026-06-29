using SquidStd.Vfs.Abstractions.Data;

namespace SquidStd.Vfs.Abstractions.Interfaces;

/// <summary>A path-based virtual filesystem over a pluggable backend (directory, zip, encrypted container).</summary>
public interface IVirtualFileSystem
{
    /// <summary>Whether a file exists at the logical path.</summary>
    ValueTask<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Reads the whole file, or null when it does not exist.</summary>
    ValueTask<byte[]?> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Writes the whole file, creating or overwriting it.</summary>
    ValueTask WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);

    /// <summary>Opens a readable stream over the file; throws <see cref="FileNotFoundException" /> when absent.</summary>
    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Opens a writable stream that creates or overwrites the file on disposal.</summary>
    Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Deletes the file. Returns whether one was removed.</summary>
    ValueTask<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Lists entries, optionally filtered by logical path prefix.</summary>
    IAsyncEnumerable<VfsEntry> ListAsync(string? prefix = null, CancellationToken cancellationToken = default);
}
