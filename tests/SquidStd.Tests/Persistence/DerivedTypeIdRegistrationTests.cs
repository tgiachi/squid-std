using SquidStd.Persistence.Abstractions;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.MessagePack;
using SquidStd.Persistence.Services;

namespace SquidStd.Tests.Persistence;

public class DerivedTypeIdRegistrationTests
{
    private sealed class Thing
    {
        public int Id { get; set; }
    }

    [Fact]
    public void DerivedId_IsTheHashOfTheStoreName()
    {
        var registry = new PersistenceEntityRegistry();

        Descriptor(registry, "accounts");

        Assert.True(registry.IsRegistered(PersistedTypeId.Derive("accounts")));
    }

    [Fact]
    public void Descriptor_RecordsNoLegacyIdByDefault()
    {
        var registry = new PersistenceEntityRegistry();

        Descriptor(registry, "accounts");

        Assert.Null(registry.GetDescriptor(PersistedTypeId.Derive("accounts")).LegacyTypeId);
    }

    [Fact]
    public void Descriptor_CarriesTheDeclaredLegacyId()
    {
        var registry = new PersistenceEntityRegistry();

        Descriptor(registry, "accounts", legacyTypeId: 1);

        Assert.Equal((ushort)1, registry.GetDescriptor(PersistedTypeId.Derive("accounts")).LegacyTypeId);
    }

    [Fact]
    public void CollisionMessage_NamesBothStores()
    {
        var registry = new PersistenceEntityRegistry();
        Descriptor(registry, "accounts");

        // Registering a second store under the same id is what a colliding hash looks like from the
        // registry's side. The message has to name both, because "type id 47467 is already registered"
        // tells the reader nothing they can act on.
        var exception = Assert.Throws<InvalidOperationException>(
            () => Descriptor(registry, "ledgers", pinnedId: PersistedTypeId.Derive("accounts"))
        );

        Assert.Contains("accounts", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ledgers", exception.Message, StringComparison.Ordinal);
    }

    private static void Descriptor(
        PersistenceEntityRegistry registry,
        string storeName,
        ushort? legacyTypeId = null,
        ushort? pinnedId = null
    )
    {
        var serializer = new MessagePackDataSerializer();

        registry.Register(
            new PersistenceEntityDescriptor<Thing, int>(
                serializer,
                serializer,
                pinnedId ?? PersistedTypeId.Derive(storeName),
                storeName,
                1,
                thing => thing.Id,
                null,
                null,
                legacyTypeId
            )
        );
    }
}
