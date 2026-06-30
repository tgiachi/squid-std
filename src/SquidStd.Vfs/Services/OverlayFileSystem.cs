using System.Runtime.CompilerServices;
using SquidStd.Vfs.Abstractions.Data;
using SquidStd.Vfs.Abstractions.Interfaces;

namespace SquidStd.Vfs.Services;

/// <summary>Reads from an overlay first then a base; writes and deletes affect only the overlay.</summary>
public sealed class OverlayFileSystem : IVirtualFileSystem
{
    private readonly IVirtualFileSystem _base;
    private readonly IVirtualFileSystem _overlay;

    public OverlayFileSystem(IVirtualFileSystem baseFileSystem, IVirtualFileSystem overlay)
    {
        ArgumentNullException.ThrowIfNull(baseFileSystem);
        ArgumentNullException.ThrowIfNull(overlay);

        _base = baseFileSystem;
        _overlay = overlay;
    }

    public async ValueTask<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        => await _overlay.ExistsAsync(path, cancellationToken).ConfigureAwait(false)
           || await _base.ExistsAsync(path, cancellationToken).ConfigureAwait(false);

    public async ValueTask<byte[]?> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        => await _overlay.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false)
           ?? await _base.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

    public ValueTask WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        => _overlay.WriteAllBytesAsync(path, data, cancellationToken);

    public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        => await _overlay.ExistsAsync(path, cancellationToken).ConfigureAwait(false)
               ? await _overlay.OpenReadAsync(path, cancellationToken).ConfigureAwait(false)
               : await _base.OpenReadAsync(path, cancellationToken).ConfigureAwait(false);

    public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        => _overlay.OpenWriteAsync(path, cancellationToken);

    public ValueTask<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
        => _overlay.DeleteAsync(path, cancellationToken);

    public async IAsyncEnumerable<VfsEntry> ListAsync(string? prefix = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var entry in _overlay.ListAsync(prefix, cancellationToken).ConfigureAwait(false))
        {
            seen.Add(entry.Path);

            yield return entry;
        }

        await foreach (var entry in _base.ListAsync(prefix, cancellationToken).ConfigureAwait(false))
        {
            if (seen.Add(entry.Path))
            {
                yield return entry;
            }
        }
    }
}
