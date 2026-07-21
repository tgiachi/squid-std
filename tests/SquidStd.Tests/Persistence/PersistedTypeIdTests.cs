using SquidStd.Persistence.Abstractions;

namespace SquidStd.Tests.Persistence;

public class PersistedTypeIdTests
{
    [Theory]
    [InlineData("accounts", 47467)]
    [InlineData("mobiles", 45233)]
    [InlineData("items", 2550)]
    [InlineData("server_settings", 26446)]
    [InlineData("news", 63361)]
    public void Derive_IsPinnedToKnownValues(string storeName, int expected)
        // Golden values. The id is written into every journal record and into the snapshot file name,
        // so changing this function silently orphans every existing save. If this test fails, the
        // algorithm changed and the change is a breaking one.
        => Assert.Equal((ushort)expected, PersistedTypeId.Derive(storeName));

    [Fact]
    public void Derive_IsStableAcrossCalls()
        => Assert.Equal(PersistedTypeId.Derive("accounts"), PersistedTypeId.Derive("accounts"));

    [Fact]
    public void Derive_IsCaseSensitive()
        // The store name reaches the file system, so treating case as significant matches how it is
        // already used rather than inventing a second rule.
        => Assert.NotEqual(PersistedTypeId.Derive("accounts"), PersistedTypeId.Derive("Accounts"));

    [Theory]
    [InlineData("accounts")]
    [InlineData("mobiles")]
    [InlineData("a")]
    [InlineData("a_very_long_store_name_that_goes_on_and_on_for_a_while")]
    public void Derive_NeverReturnsAReservedValue(string storeName)
    {
        var id = PersistedTypeId.Derive(storeName);

        // 0 means "unassigned"; 65535 is the internal id-sequence bucket and RegisterPersistedEntity
        // already rejects it.
        Assert.NotEqual(0, id);
        Assert.NotEqual(ushort.MaxValue, id);
    }

    [Fact]
    public void Derive_RejectsAnEmptyName()
        => Assert.Throws<ArgumentException>(() => PersistedTypeId.Derive(string.Empty));
}
