using System.Runtime.CompilerServices;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SquidStd.Vfs.Abstractions;
using SquidStd.Vfs.Abstractions.Data;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.S3.Data;

namespace SquidStd.Vfs.S3.Services;

/// <summary>
/// S3-compatible <see cref="IVirtualFileSystem" /> backed by the MinIO client.
/// The bucket is created lazily on first use and the raw object bytes map directly to file contents.
/// </summary>
public sealed class S3FileSystem : IVirtualFileSystem, IDisposable
{
    private readonly string _bucket;
    private readonly SemaphoreSlim _bucketLock = new(1, 1);
    private readonly IMinioClient _client;
    private bool _bucketReady;
    private int _disposed;

    /// <summary>Initializes a new <see cref="S3FileSystem" /> from <paramref name="options" />.</summary>
    /// <exception cref="ArgumentException">Thrown when any required connection field is empty.</exception>
    public S3FileSystem(S3FileSystemOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Aws.ServiceUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Aws.AccessKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Aws.SecretKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Bucket);

        _client = CreateClient(options);
        _bucket = options.Bucket;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await EnsureBucketAsync(cancellationToken);

        try
        {
            await _client.StatObjectAsync(
                new StatObjectArgs().WithBucket(_bucket).WithObject(path),
                cancellationToken
            );

            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async ValueTask<byte[]?> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await EnsureBucketAsync(cancellationToken);

        using var buffer = new MemoryStream();

        try
        {
            await _client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(path)
                    .WithCallbackStream(async (stream, ct) => await stream.CopyToAsync(buffer, ct)),
                cancellationToken
            );
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }

        return buffer.ToArray();
    }

    /// <inheritdoc />
    public async ValueTask WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await EnsureBucketAsync(cancellationToken);

        var bytes = data.ToArray();
        using var stream = new MemoryStream(bytes, false);

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(path)
                .WithStreamData(stream)
                .WithObjectSize(bytes.LongLength),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await EnsureBucketAsync(cancellationToken);

        var buffer = new MemoryStream();

        try
        {
            await _client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(path)
                    .WithCallbackStream(async (stream, ct) => await stream.CopyToAsync(buffer, ct)),
                cancellationToken
            );
        }
        catch (ObjectNotFoundException)
        {
            await buffer.DisposeAsync();
            throw new FileNotFoundException($"Object not found in S3 bucket '{_bucket}': '{path}'.", path);
        }

        buffer.Position = 0;
        return buffer;
    }

    /// <inheritdoc />
    public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return Task.FromResult<Stream>(
            new DeferredWriteStream((bytes, ct) => WriteAllBytesAsync(path, bytes, ct), cancellationToken)
        );
    }

    /// <inheritdoc />
    public async ValueTask<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!await ExistsAsync(path, cancellationToken))
        {
            return false;
        }

        await _client.RemoveObjectAsync(
            new RemoveObjectArgs().WithBucket(_bucket).WithObject(path),
            cancellationToken
        );

        return true;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VfsEntry> ListAsync(
        string? prefix = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await EnsureBucketAsync(cancellationToken);

        var args = new ListObjectsArgs().WithBucket(_bucket).WithRecursive(true);

        if (!string.IsNullOrEmpty(prefix))
        {
            args = args.WithPrefix(prefix);
        }

        await foreach (var item in _client.ListObjectsEnumAsync(args, cancellationToken))
        {
            if (!item.IsDir)
            {
                yield return new VfsEntry(
                    item.Key,
                    (long)item.Size,
                    item.LastModifiedDateTime?.ToUniversalTime() ?? default
                );
            }
        }
    }

    private static IMinioClient CreateClient(S3FileSystemOptions options)
    {
        var uri = new Uri(options.Aws.ServiceUrl!, UriKind.Absolute);
        var endpoint = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";

        var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(options.Aws.AccessKey, options.Aws.SecretKey)
                    .WithSSL(string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(options.Aws.Region))
        {
            minio = minio.WithRegion(options.Aws.Region);
        }

        return minio.Build();
    }

    private async ValueTask EnsureBucketAsync(CancellationToken cancellationToken)
    {
        await _bucketLock.WaitAsync(cancellationToken);

        try
        {
            if (_bucketReady)
            {
                return;
            }

            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucket),
                cancellationToken
            );

            if (!exists)
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucket),
                    cancellationToken
                );
            }

            _bucketReady = true;
        }
        finally
        {
            _bucketLock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _client.Dispose();
        _bucketLock.Dispose();
    }
}
