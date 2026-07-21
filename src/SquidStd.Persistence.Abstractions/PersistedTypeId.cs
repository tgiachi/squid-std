namespace SquidStd.Persistence.Abstractions;

/// <summary>
/// Derives a persisted entity's type id from its store name.
/// </summary>
/// <remarks>
/// The algorithm is written out rather than delegated to <see cref="string.GetHashCode()" />, which is
/// randomized per process in .NET and would hand out a different id on every run. The id is written
/// into every journal record and into the snapshot file name, so it must be identical on every machine
/// and every release: treat this function as a wire format, not as an implementation detail.
/// </remarks>
public static class PersistedTypeId
{
    private const uint OffsetBasis = 2166136261;
    private const uint Prime = 16777619;

    /// <summary>
    /// Returns the type id for <paramref name="storeName" />: FNV-1a over its characters, folded from
    /// 32 to 16 bits. Never returns 0, which means "unassigned", nor <see cref="ushort.MaxValue" />,
    /// which is reserved for the internal id-sequence bucket.
    /// </summary>
    public static ushort Derive(string storeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);

        var hash = OffsetBasis;

        foreach (var character in storeName)
        {
            hash ^= character;
            hash *= Prime;
        }

        var folded = (ushort)((hash >> 16) ^ (hash & 0xFFFF));

        // Nudged rather than rejected: the two reserved values are simply mapped to their neighbours,
        // which costs a marginally higher collision chance on exactly two ids and keeps the function
        // total.
        return folded switch
        {
            0               => 1,
            ushort.MaxValue => ushort.MaxValue - 1,
            _               => folded
        };
    }
}
