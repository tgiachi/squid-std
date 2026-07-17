using SquidStd.Core.Json;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.Internal;
using SquidStd.Persistence.Services;

namespace SquidStd.Tests.Persistence;

public sealed class EntityStorePagedTests : IAsyncDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "squidstd-paged-" + Guid.NewGuid().ToString("N"));
    private readonly CountingDescriptor _descriptor;
    private readonly BinaryJournalService _journal;
    private readonly PersistenceStateStore _stateStore = new();
    private readonly EntityStore<Player, int> _store;

    public EntityStorePagedTests()
    {
        var serializer = new JsonDataSerializer();
        _descriptor = new(
            new PersistenceEntityDescriptor<Player, int>(serializer, serializer, 1, "Player", 1, p => p.Id)
        );
        _journal = new(Path.Combine(_dir, "world.journal.bin"));
        _store = new(_stateStore, _journal, _descriptor);
    }

    private async Task SeedAsync(params (int Id, string Name)[] players)
    {
        foreach (var (id, name) in players)
        {
            await _store.UpsertAsync(new() { Id = id, Name = name });
        }
    }

    [Fact]
    public async Task QueryPaged_OrdersAndPages()
    {
        await SeedAsync((1, "Carol"), (2, "Alice"), (3, "Bob"));

        var page = _store.QueryPaged(null, p => p.Name, 0, 2);

        Assert.Equal(["Alice", "Bob"], page.Items.Select(p => p.Name));
        Assert.Equal(3, page.Total);
        Assert.Equal(0, page.Skip);
        Assert.Equal(2, page.Take);
    }

    [Fact]
    public async Task QueryPaged_Descending_ReversesTheOrder()
    {
        await SeedAsync((1, "Carol"), (2, "Alice"), (3, "Bob"));

        var page = _store.QueryPaged(null, p => p.Name, 0, 2, true);

        Assert.Equal(["Carol", "Bob"], page.Items.Select(p => p.Name));
    }

    [Fact]
    public async Task QueryPaged_Total_CountsTheFilterNotThePage()
    {
        await SeedAsync((1, "Alice"), (2, "Amy"), (3, "Bob"));

        var page = _store.QueryPaged(p => p.Name.StartsWith('A'), p => p.Name, 0, 1);

        Assert.Single(page.Items);
        Assert.Equal(2, page.Total); // Alice and Amy matched; only one is on the page
    }

    [Fact]
    public async Task QueryPaged_SkipPastTheEnd_IsEmptyButKeepsTheTotal()
    {
        await SeedAsync((1, "Alice"), (2, "Bob"));

        var page = _store.QueryPaged(null, p => p.Name, 99, 10);

        Assert.Empty(page.Items);
        Assert.Equal(2, page.Total);
    }

    [Fact]
    public async Task QueryPaged_ReturnsDetachedClones()
    {
        await SeedAsync((1, "Alice"));

        var page = _store.QueryPaged(null, p => p.Name, 0, 10);
        page.Items[0].Tags.Add("mutated");

        var again = _store.QueryPaged(null, p => p.Name, 0, 10);

        Assert.Empty(again.Items[0].Tags);
    }

    [Fact]
    public async Task QueryPaged_ClonesOnlyThePage()
    {
        // The entire point of this method. GetAll and Query deep-clone the whole store on every call; if
        // this did the same there would be no reason for it to exist, and no other assertion would notice.
        await SeedAsync((1, "A"), (2, "B"), (3, "C"), (4, "D"), (5, "E"));
        _descriptor.CloneCount = 0;

        var page = _store.QueryPaged(null, p => p.Name, 0, 2);

        Assert.Equal(2, page.Items.Count);
        Assert.Equal(2, _descriptor.CloneCount);
    }

    [Fact]
    public async Task QueryPaged_TiedOrderKeys_AreBrokenByEntityKey()
    {
        // Two entities sorting equal must not be left to Dictionary order, or page 2 could repeat page 1's
        // row and drop another, with nothing in the response admitting it.
        await SeedAsync((3, "Same"), (1, "Same"), (2, "Same"));

        var first = _store.QueryPaged(null, p => p.Name, 0, 3);
        var second = _store.QueryPaged(null, p => p.Name, 0, 3);

        Assert.Equal([1, 2, 3], first.Items.Select(p => p.Id));
        Assert.Equal(first.Items.Select(p => p.Id), second.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task QueryPaged_PagesDoNotOverlapOrSkipWhenKeysTie()
    {
        await SeedAsync((1, "Same"), (2, "Same"), (3, "Same"), (4, "Same"));

        var first = _store.QueryPaged(null, p => p.Name, 0, 2);
        var next = _store.QueryPaged(null, p => p.Name, 2, 2);

        Assert.Equal([1, 2], first.Items.Select(p => p.Id));
        Assert.Equal([3, 4], next.Items.Select(p => p.Id));
    }

    public async ValueTask DisposeAsync()
    {
        await _journal.DisposeAsync();

        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }

    /// <summary>Wraps a real descriptor and counts Clone calls, so a test can prove what was cloned.</summary>
    private sealed class CountingDescriptor : IPersistenceEntityDescriptor<Player, int>
    {
        private readonly IPersistenceEntityDescriptor<Player, int> _inner;

        public CountingDescriptor(IPersistenceEntityDescriptor<Player, int> inner)
        {
            _inner = inner;
        }

        public int CloneCount { get; set; }

        public ushort TypeId => _inner.TypeId;

        public string TypeName => _inner.TypeName;

        public int SchemaVersion => _inner.SchemaVersion;

        public Type EntityType => _inner.EntityType;

        public Type KeyType => _inner.KeyType;

        public int GetKey(Player entity)
            => _inner.GetKey(entity);

        public Player Clone(Player entity)
        {
            CloneCount++;

            return _inner.Clone(entity);
        }

        public byte[] SerializeEntity(Player entity)
            => _inner.SerializeEntity(entity);

        public Player DeserializeEntity(byte[] payload)
            => _inner.DeserializeEntity(payload);

        public byte[] SerializeBucket(IReadOnlyCollection<Player> entities)
            => _inner.SerializeBucket(entities);

        public IReadOnlyList<Player> DeserializeBucket(byte[] payload)
            => _inner.DeserializeBucket(payload);

        public byte[] SerializeKey(int key)
            => _inner.SerializeKey(key);

        public int DeserializeKey(byte[] payload)
            => _inner.DeserializeKey(payload);
    }

    private sealed class Player
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = [];
    }
}
