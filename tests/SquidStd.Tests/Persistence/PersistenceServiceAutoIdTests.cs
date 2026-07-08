using SquidStd.Core.Json;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.Services;

namespace SquidStd.Tests.Persistence;

public sealed class PersistenceServiceAutoIdTests : IDisposable
{
    private sealed class Doc
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "squidstd-svc-autoid-" + Guid.NewGuid().ToString("N"));

    private (PersistenceService Service, IEntityStore<Doc, int> Store, BinaryJournalService Journal) NewService()
    {
        var serializer = new JsonDataSerializer();
        var registry = new PersistenceEntityRegistry();
        registry.Register(new PersistenceEntityDescriptor<Doc, int>(
            serializer, serializer, 7, "Doc", 1,
            keySelector: d => d.Id,
            keySetter: (d, id) => d.Id = id,
            idGenerator: IdGenerators.Int32(seed: 1)));

        var config = new PersistenceConfig { SaveDirectory = _dir, AutosaveInterval = TimeSpan.FromMinutes(30) };
        var journal = new BinaryJournalService(Path.Combine(_dir, config.JournalFileName));
        var snapshot = new SnapshotService(_dir, config.SnapshotFileSuffix);
        var service = new PersistenceService(registry, journal, snapshot, config);

        return (service, service.GetStore<Doc, int>(), journal);
    }

    [Fact]
    public async Task Ids_ContinueAcrossSnapshotAndReload()
    {
        var (service, store, journal) = NewService();
        await service.StartAsync();
        var a = new Doc { Name = "a" };
        var b = new Doc { Name = "b" };
        await store.UpsertAsync(a); // 1
        await store.UpsertAsync(b); // 2
        await service.StopAsync();  // writes snapshot, trims journal
        await journal.DisposeAsync();

        var (service2, store2, journal2) = NewService();
        await service2.StartAsync(); // loads snapshot (incl. high-water)
        var c = new Doc { Name = "c" };
        await store2.UpsertAsync(c);
        await service2.StopAsync();
        await journal2.DisposeAsync();

        Assert.Equal(3, c.Id); // did not restart at 1
    }

    [Fact]
    public async Task DeletingHighestThenReloading_DoesNotReuseId()
    {
        var (service, store, journal) = NewService();
        await service.StartAsync();
        var a = new Doc { Name = "a" }; // 1
        var b = new Doc { Name = "b" }; // 2
        await store.UpsertAsync(a);
        await store.UpsertAsync(b);
        await store.RemoveAsync(2);     // delete the highest
        await service.StopAsync();
        await journal.DisposeAsync();

        var (service2, store2, journal2) = NewService();
        await service2.StartAsync();
        var c = new Doc { Name = "c" };
        await store2.UpsertAsync(c);
        await service2.StopAsync();
        await journal2.DisposeAsync();

        Assert.Equal(3, c.Id); // NOT 2 — the high-water did not regress
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }
}
