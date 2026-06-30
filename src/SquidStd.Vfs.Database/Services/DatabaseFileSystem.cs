using System.Runtime.CompilerServices;
using SquidStd.Database.Abstractions.Interfaces.Data;
using SquidStd.Vfs.Abstractions;
using SquidStd.Vfs.Abstractions.Data;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.Database.Data.Entities;

namespace SquidStd.Vfs.Database.Services;

/// <summary>
/// A <see cref="IVirtualFileSystem" /> implementation that stores files as rows in a relational
/// database via the generic <see cref="IDataAccess{TEntity}" /> abstraction (FreeSql).
/// </summary>
public sealed class DatabaseFileSystem : IVirtualFileSystem
{
    private readonly IDataAccess<VfsFileEntity> _data;

    /// <summary>
    /// Initializes the database filesystem.
    /// </summary>
    /// <param name="dataAccess">The data access for <see cref="VfsFileEntity" /> rows.</param>
    public DatabaseFileSystem(IDataAccess<VfsFileEntity> dataAccess)
    {
        _data = dataAccess;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return await _data.ExistsAsync(e => e.Path == path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<byte[]?> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var rows = await _data.QueryAsync(e => e.Path == path, cancellationToken).ConfigureAwait(false);

        return rows.Count > 0 ? rows[0].Content : null;
    }

    /// <inheritdoc />
    public async ValueTask WriteAllBytesAsync(
        string path,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var rows = await _data.QueryAsync(e => e.Path == path, cancellationToken).ConfigureAwait(false);
        var existing = rows.Count > 0 ? rows[0] : null;
        var bytes = data.ToArray();

        if (existing is not null)
        {
            existing.Content = bytes;
            existing.Size = bytes.Length;
            await _data.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _data.InsertAsync(
                new VfsFileEntity { Path = path, Content = bytes, Size = bytes.Length },
                cancellationToken
            ).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var bytes = await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false)
                    ?? throw new FileNotFoundException($"No file at '{path}'.", path);

        return new MemoryStream(bytes, writable: false);
    }

    /// <inheritdoc />
    public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Task.FromResult<Stream>(
            new DeferredWriteStream(
                (bytes, ct) => WriteAllBytesAsync(path, bytes, ct),
                cancellationToken
            )
        );
    }

    /// <inheritdoc />
    public async ValueTask<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return await _data.BulkDeleteAsync(e => e.Path == path, cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VfsEntry> ListAsync(
        string? prefix = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var rows = string.IsNullOrEmpty(prefix)
            ? await _data.QueryAsync(cancellationToken: cancellationToken).ConfigureAwait(false)
            : await _data.QueryAsync(e => e.Path.StartsWith(prefix), cancellationToken).ConfigureAwait(false);

        foreach (var e in rows)
        {
            yield return new VfsEntry(e.Path, e.Size, e.Updated);
        }
    }
}
