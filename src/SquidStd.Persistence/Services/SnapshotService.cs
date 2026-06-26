using Serilog;
using SquidStd.Core.Utils;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Internal;
using ILogger = Serilog.ILogger;

namespace SquidStd.Persistence.Services;

/// <summary>
/// Stores each registered entity type as its own fixed-binary snapshot file
/// (<c>&lt;snake_case type name&gt;&lt;suffix&gt;</c> under the save directory), written atomically
/// via temp + rename and verified by a payload checksum on load.
/// </summary>
public sealed class SnapshotService : ISnapshotService, IDisposable
{
    private static readonly char[] _invalidTypeNameChars =
        [.. Path.GetInvalidFileNameChars(), Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    private readonly string _directory;
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly ILogger _logger = Log.ForContext<SnapshotService>();
    private readonly string _suffix;

    public SnapshotService(string saveDirectory, string fileSuffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileSuffix);

        _directory = Path.GetFullPath(saveDirectory);
        _suffix = fileSuffix;

        Directory.CreateDirectory(_directory);
    }

    public async ValueTask DeleteBucketAsync(string typeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        var path = PathFor(typeName);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            File.Delete(path);
            File.Delete(path + ".tmp");
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask<PersistedBucket?> LoadBucketAsync(string typeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        var path = PathFor(typeName);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var envelope = SnapshotEnvelopeCodec.Decode(bytes);

            if (ChecksumUtils.Compute(envelope.Bucket.Payload) != envelope.Checksum ||
                !string.Equals(envelope.Bucket.TypeName, typeName, StringComparison.Ordinal))
            {
                _logger.Error("Snapshot {Path}: checksum or type-name mismatch; treating as absent", path);

                return null;
            }

            return new PersistedBucket(envelope.Bucket, envelope.LastSequenceId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Snapshot {Path}: unreadable; treating as absent", path);

            return null;
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask SaveBucketAsync(
        EntitySnapshotBucket bucket, long lastSequenceId, CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(bucket);

        var envelope = new SnapshotFileEnvelope
        {
            Version = 1,
            LastSequenceId = lastSequenceId,
            Checksum = ChecksumUtils.Compute(bucket.Payload),
            Bucket = bucket
        };

        var path = PathFor(bucket.TypeName);
        var tempPath = path + ".tmp";
        var bytes = SnapshotEnvelopeCodec.Encode(envelope);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(tempPath, path, true);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    private string PathFor(string typeName)
    {
        if (typeName.AsSpan().IndexOfAny(_invalidTypeNameChars) >= 0)
        {
            throw new InvalidOperationException($"Persisted type name '{typeName}' cannot be used as a snapshot file name.");
        }

        return Path.Combine(_directory, StringUtils.ToSnakeCase(typeName) + _suffix);
    }

    public void Dispose()
        => _ioLock.Dispose();
}
