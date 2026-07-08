using SquidStd.Core.Json;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.Internal;

namespace SquidStd.Tests.Persistence;

public class PersistenceEntityDescriptorAutoIdTests
{
    private sealed class Doc
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private static PersistenceEntityDescriptor<Doc, int> AutoDescriptor()
    {
        var serializer = new JsonDataSerializer();
        return new PersistenceEntityDescriptor<Doc, int>(
            serializer, serializer, 7, "Doc", 1,
            keySelector: d => d.Id,
            keySetter: (d, id) => d.Id = id,
            idGenerator: IdGenerators.Int32(seed: 1));
    }

    [Fact]
    public void ManualDescriptor_IsNotAutoId()
    {
        var serializer = new JsonDataSerializer();
        var descriptor = new PersistenceEntityDescriptor<Doc, int>(serializer, serializer, 7, "Doc", 1, d => d.Id);

        Assert.False(descriptor.IsAutoId);
    }

    [Fact]
    public void AutoDescriptor_AllocatesInitialThenNext()
    {
        var descriptor = AutoDescriptor();
        var store = new PersistenceStateStore();

        Assert.True(descriptor.IsAutoId);
        Assert.Equal(1, descriptor.AllocateNextKey(store));
        Assert.Equal(2, descriptor.AllocateNextKey(store));
    }

    [Fact]
    public void NoteKey_AdvancesHighWater_SoNextAllocationSkipsPast()
    {
        var descriptor = AutoDescriptor();
        var store = new PersistenceStateStore();

        descriptor.NoteKey(store, 40);

        Assert.Equal(41, descriptor.AllocateNextKey(store));
    }

    [Fact]
    public void NoteKey_NeverLowersHighWater()
    {
        var descriptor = AutoDescriptor();
        var store = new PersistenceStateStore();

        descriptor.NoteKey(store, 40);
        descriptor.NoteKey(store, 10);

        Assert.Equal(41, descriptor.AllocateNextKey(store));
    }

    [Fact]
    public void IsDefaultKey_TrueForZero_FalseForSet()
    {
        var descriptor = AutoDescriptor();

        Assert.True(descriptor.IsDefaultKey(0));
        Assert.False(descriptor.IsDefaultKey(5));
    }

    [Fact]
    public void SetKey_WritesOntoEntity()
    {
        var descriptor = AutoDescriptor();
        var doc = new Doc();

        descriptor.SetKey(doc, 99);

        Assert.Equal(99, doc.Id);
    }

    [Fact]
    public void HighWater_RoundTripsThroughSerialize()
    {
        var descriptor = AutoDescriptor();
        var store = new PersistenceStateStore();
        descriptor.NoteKey(store, 40);

        var payload = ((IInternalEntityApplier)descriptor).SerializeHighWater(store);
        Assert.NotNull(payload);

        var restored = new PersistenceStateStore();
        ((IInternalEntityApplier)descriptor).LoadHighWater(restored, payload!);

        Assert.Equal(41, descriptor.AllocateNextKey(restored));
    }
}
