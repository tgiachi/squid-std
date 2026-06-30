using SquidStd.Vfs.Abstractions.Data;
using SquidStd.Vfs.Abstractions.Interfaces;

namespace SquidStd.Vfs.Services;

/// <summary>Wraps a filesystem and rejects every mutation.</summary>
public sealed class ReadOnlyFileSystem : IVirtualFileSystem
{
    private const string Message = "This filesystem is read-only.";

    private readonly IVirtualFileSystem _inner;

    public ReadOnlyFileSystem(IVirtualFileSystem inner)
    {
        ArgumentNullException.ThrowIfNull(inner);

        _inner = inner;
    }

    public ValueTask<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        => _inner.ExistsAsync(path, cancellationToken);

    public ValueTask<byte[]?> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        => _inner.ReadAllBytesAsync(path, cancellationToken);

    public ValueTask WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        => _inner.OpenReadAsync(path, cancellationToken);

    public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public ValueTask<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public IAsyncEnumerable<VfsEntry> ListAsync(string? prefix = null, CancellationToken cancellationToken = default)
        => _inner.ListAsync(prefix, cancellationToken);
}
