namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>
/// Produces successive keys for an auto-id entity type: <see cref="Initial" /> is the first key
/// handed out, and <see cref="Next" /> returns the key after a given one.
/// </summary>
public interface IIdGenerator<TKey>
    where TKey : notnull
{
    /// <summary>The first key to allocate when nothing has been allocated yet.</summary>
    TKey Initial { get; }

    /// <summary>The key that follows <paramref name="current" />.</summary>
    TKey Next(TKey current);
}
