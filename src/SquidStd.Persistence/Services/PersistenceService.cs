using Serilog;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Data.Events;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Abstractions.Types;
using SquidStd.Persistence.Internal;
using ILogger = Serilog.ILogger;

namespace SquidStd.Persistence.Services;

/// <summary>
/// Owns persistence lifecycle: loads snapshots and replays the journal tail at startup, autosaves on a
/// timer, and hands out per-type <see cref="IEntityStore{TEntity,TKey}" /> instances.
/// </summary>
public sealed class PersistenceService : IPersistenceService, IAsyncDisposable
{
    private readonly PersistenceConfig _config;
    private readonly IEventBus? _eventBus;
    private readonly IJournalService _journalService;
    private readonly ILogger _logger = Log.ForContext<PersistenceService>();
    private readonly IPersistenceEntityRegistry _registry;
    private readonly IReadOnlyList<IPersistenceSeeder> _seeders;
    private readonly ISnapshotService _snapshotService;
    private readonly PersistenceStateStore _stateStore = new();
    private readonly SemaphoreSlim _snapshotLock = new(1, 1);
    private CancellationTokenSource? _autosaveCts;
    private Task? _autosaveLoop;

    private const ushort HighWaterTypeId = ushort.MaxValue;
    private const string HighWaterTypeName = "__id_sequences";

    /// <param name="registry">The entity registry describing every registered persisted type.</param>
    /// <param name="journalService">Appends and replays journal entries.</param>
    /// <param name="snapshotService">Loads and saves per-type snapshot buckets.</param>
    /// <param name="config">Persistence configuration (autosave interval, save directory, etc.).</param>
    /// <param name="eventBus">Optional event bus used to publish snapshot lifecycle events.</param>
    /// <param name="seeders">
    /// Optional seeders run once, in order, right after a fresh save (no snapshot, no journal) finishes
    /// replaying in <see cref="StartAsync" />.
    /// </param>
    public PersistenceService(
        IPersistenceEntityRegistry registry,
        IJournalService journalService,
        ISnapshotService snapshotService,
        PersistenceConfig config,
        IEventBus? eventBus = null,
        IReadOnlyList<IPersistenceSeeder>? seeders = null
    )
    {
        _registry = registry;
        _journalService = journalService;
        _snapshotService = snapshotService;
        _config = config;
        _eventBus = eventBus;
        _seeders = seeders ?? [];
    }

    public IEntityStore<TEntity, TKey> GetStore<TEntity, TKey>()
        where TKey : notnull
        => new EntityStore<TEntity, TKey>(_stateStore, _journalService, _registry.GetDescriptor<TEntity, TKey>());

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        _registry.Freeze();

        var maxSequenceId = 0L;
        var snapshotThresholds = new Dictionary<ushort, long>();

        foreach (var descriptor in _registry.GetRegisteredDescriptors())
        {
            var loaded = await _snapshotService.LoadBucketAsync(
                descriptor.TypeName,
                descriptor.TypeId,
                descriptor.LegacyTypeId,
                cancellationToken
            );

            if (loaded is null)
            {
                continue;
            }

            ((IInternalEntityApplier)descriptor).LoadBucket(_stateStore, loaded.Bucket);
            snapshotThresholds[descriptor.TypeId] = loaded.LastSequenceId;
            maxSequenceId = Math.Max(maxSequenceId, loaded.LastSequenceId);
        }

        var highWater = await _snapshotService.LoadBucketAsync(HighWaterTypeName, HighWaterTypeId, cancellationToken);

        if (highWater is not null)
        {
            LoadHighWaters(highWater.Bucket.Payload);
        }

        foreach (var entry in await _journalService.ReadAllAsync(cancellationToken))
        {
            // Replay each entry against its own type's snapshot watermark, not a global maximum: a
            // single global threshold would skip journal entries newer than a lagging type's snapshot
            // (e.g. after a partial snapshot save where one bucket persisted and another did not),
            // silently dropping that type's recent writes.
            if (entry.SequenceId <= snapshotThresholds.GetValueOrDefault(entry.TypeId))
            {
                continue;
            }

            if (!_registry.IsRegistered(entry.TypeId))
            {
                _logger.Warning(
                    "Journal replay: unregistered type id {TypeId}; skipping entry {SequenceId}",
                    entry.TypeId,
                    entry.SequenceId
                );

                continue;
            }

            var applier = (IInternalEntityApplier)_registry.GetDescriptor(entry.TypeId);

            switch (entry.Operation)
            {
                case JournalEntityOperationType.Upsert:
                    applier.ApplyUpsert(_stateStore, entry.Payload);

                    break;
                case JournalEntityOperationType.Remove:
                    applier.ApplyRemove(_stateStore, entry.Payload);

                    break;
            }

            maxSequenceId = Math.Max(maxSequenceId, entry.SequenceId);
        }

        _stateStore.LastSequenceId = maxSequenceId;
    }

    public async ValueTask SaveSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // One snapshot at a time: autosave, an explicit call, and StopAsync must not interleave their
        // capture/save/trim phases, or a lower-sequence snapshot could land after a higher-sequence trim.
        await _snapshotLock.WaitAsync(cancellationToken);

        try
        {
            long capturedSequenceId;
            List<EntitySnapshotBucket> buckets = [];
            List<(string TypeName, ushort TypeId)> emptyTypes = [];
            byte[]? highWaterPayload;

            await _stateStore.WriteLock.WaitAsync(cancellationToken);

            try
            {
                capturedSequenceId = _stateStore.LastSequenceId;

                foreach (var descriptor in _registry.GetRegisteredDescriptors())
                {
                    lock (_stateStore.SyncRoot)
                    {
                        var bucket = ((IInternalEntityApplier)descriptor).CaptureBucket(_stateStore);

                        if (bucket is null)
                        {
                            emptyTypes.Add((descriptor.TypeName, descriptor.TypeId));
                        }
                        else
                        {
                            buckets.Add(bucket);
                        }
                    }
                }

                lock (_stateStore.SyncRoot)
                {
                    highWaterPayload = CaptureHighWaters();
                }
            }
            finally
            {
                _stateStore.WriteLock.Release();
            }

            if (_eventBus is not null)
            {
                await _eventBus.PublishAsync(new SnapshotSaveStartedEvent(capturedSequenceId), cancellationToken);
            }

            foreach (var bucket in buckets)
            {
                await _snapshotService.SaveBucketAsync(bucket, capturedSequenceId, cancellationToken);
            }

            foreach (var (typeName, typeId) in emptyTypes)
            {
                await _snapshotService.DeleteBucketAsync(typeName, typeId, cancellationToken);
            }

            if (highWaterPayload is not null)
            {
                await _snapshotService.SaveBucketAsync(
                    new EntitySnapshotBucket
                    {
                        TypeId = HighWaterTypeId,
                        TypeName = HighWaterTypeName,
                        SchemaVersion = 1,
                        Payload = highWaterPayload
                    },
                    capturedSequenceId,
                    cancellationToken
                );
            }
            else
            {
                await _snapshotService.DeleteBucketAsync(HighWaterTypeName, HighWaterTypeId, cancellationToken);
            }

            await _journalService.TrimThroughSequenceAsync(capturedSequenceId, cancellationToken);

            if (_eventBus is not null)
            {
                await _eventBus.PublishAsync(
                    new SnapshotSaveCompletedEvent(capturedSequenceId, buckets.Count),
                    cancellationToken
                );
            }
        }
        finally
        {
            _snapshotLock.Release();
        }
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        if (_stateStore.LastSequenceId == 0 && _seeders.Count > 0)
        {
            foreach (var seeder in _seeders)
            {
                await seeder.SeedAsync(this, cancellationToken);
            }

            _logger.Information("Seeded fresh persistence store with {Count} seeder(s)", _seeders.Count);
        }

        _autosaveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _autosaveLoop = Task.Run(() => AutosaveLoopAsync(_autosaveCts.Token), CancellationToken.None);
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_autosaveCts is not null)
        {
            await _autosaveCts.CancelAsync();
        }

        if (_autosaveLoop is not null)
        {
            try
            {
                await _autosaveLoop;
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
        }

        await SaveSnapshotAsync(cancellationToken);

        _autosaveCts?.Dispose();
        _autosaveCts = null;
        _autosaveLoop = null;
    }

    private async Task AutosaveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_config.AutosaveInterval, cancellationToken);
                await SaveSnapshotAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Autosave failed; journal retained for the next attempt");
            }
        }
    }

    private byte[]? CaptureHighWaters()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(0); // placeholder count
        var count = 0;

        foreach (var descriptor in _registry.GetRegisteredDescriptors())
        {
            var applier = (IInternalEntityApplier)descriptor;

            if (!applier.IsAutoId)
            {
                continue;
            }

            var keyBytes = applier.SerializeHighWater(_stateStore);

            if (keyBytes is null)
            {
                continue;
            }

            writer.Write(descriptor.TypeId);
            writer.Write(keyBytes.Length);
            writer.Write(keyBytes);
            count++;
        }

        if (count == 0)
        {
            return null;
        }

        writer.Flush();
        stream.Position = 0;
        new BinaryWriter(stream).Write(count);

        return stream.ToArray();
    }

    private void LoadHighWaters(byte[] payload)
    {
        using var stream = new MemoryStream(payload);
        using var reader = new BinaryReader(stream);

        var count = reader.ReadInt32();

        for (var i = 0; i < count; i++)
        {
            var typeId = reader.ReadUInt16();
            var length = reader.ReadInt32();
            var keyBytes = reader.ReadBytes(length);

            if (_registry.IsRegistered(typeId))
            {
                ((IInternalEntityApplier)_registry.GetDescriptor(typeId)).LoadHighWater(_stateStore, keyBytes);
            }
        }
    }

    public async ValueTask DisposeAsync()
        => await StopAsync(CancellationToken.None);
}
