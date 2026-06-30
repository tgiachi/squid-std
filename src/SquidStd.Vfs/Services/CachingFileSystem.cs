using System.Net.Http;
using System.Runtime.CompilerServices;
using SquidStd.Vfs.Abstractions;
using SquidStd.Vfs.Abstractions.Data;
using SquidStd.Vfs.Abstractions.Interfaces;

namespace SquidStd.Vfs.Services;

/// <summary>
/// Read-through cache over a remote filesystem. Reads prefer the remote and refresh the cache; on a
/// transport failure they fall back to the (possibly stale) cache. Writes and deletes are write-through
/// (remote then cache) and surface the remote's error when it is unreachable.
/// </summary>
public sealed class CachingFileSystem : IVirtualFileSystem
{
    private readonly IVirtualFileSystem _remote;
    private readonly IVirtualFileSystem _cache;

    public CachingFileSystem(IVirtualFileSystem remote, IVirtualFileSystem cache)
    {
        ArgumentNullException.ThrowIfNull(remote);
        ArgumentNullException.ThrowIfNull(cache);

        _remote = remote;
        _cache = cache;
    }

    public async ValueTask<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _remote.ExistsAsync(path, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            return await _cache.ExistsAsync(path, cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask<byte[]?> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _remote.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

            if (data is not null)
            {
                await _cache.WriteAllBytesAsync(path, data, cancellationToken).ConfigureAwait(false);
            }

            return data;
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            return await _cache.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        await _remote.WriteAllBytesAsync(path, data, cancellationToken).ConfigureAwait(false);
        await _cache.WriteAllBytesAsync(path, data, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false)
                   ?? throw new FileNotFoundException($"No file at '{path}'.", path);

        return new MemoryStream(data, writable: false);
    }

    public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(new DeferredWriteStream((bytes, ct) => WriteAllBytesAsync(path, bytes, ct), cancellationToken));

    public async ValueTask<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var removed = await _remote.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
        await _cache.DeleteAsync(path, cancellationToken).ConfigureAwait(false);

        return removed;
    }

    public async IAsyncEnumerable<VfsEntry> ListAsync(string? prefix = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IAsyncEnumerator<VfsEntry>? enumerator = null;
        var fellBack = false;

        try
        {
            try
            {
                enumerator = _remote.ListAsync(prefix, cancellationToken).GetAsyncEnumerator(cancellationToken);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                fellBack = true;
            }

            while (!fellBack)
            {
                VfsEntry current;

                try
                {
                    if (!await enumerator!.MoveNextAsync().ConfigureAwait(false))
                    {
                        break;
                    }

                    current = enumerator.Current;
                }
                catch (Exception ex) when (IsTransient(ex))
                {
                    fellBack = true;

                    break;
                }

                yield return current;
            }
        }
        finally
        {
            if (enumerator is not null)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        if (fellBack)
        {
            await foreach (var entry in _cache.ListAsync(prefix, cancellationToken).ConfigureAwait(false))
            {
                yield return entry;
            }
        }
    }

    private static bool IsTransient(Exception ex)
        => ex is HttpRequestException or IOException or TimeoutException;
}
