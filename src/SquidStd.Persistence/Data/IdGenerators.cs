using SquidStd.Persistence.Abstractions.Interfaces.Persistence;

namespace SquidStd.Persistence.Data;

/// <summary>Built-in <see cref="IIdGenerator{TKey}" /> factories for the common integral key types.</summary>
public static class IdGenerators
{
    /// <summary>A monotonic <see cref="int" /> generator starting at <paramref name="seed" />.</summary>
    public static IIdGenerator<int> Int32(int seed = 1)
    {
        return new DelegateIdGenerator<int>(seed, current => current + 1);
    }

    /// <summary>A monotonic <see cref="long" /> generator starting at <paramref name="seed" />.</summary>
    public static IIdGenerator<long> Int64(long seed = 1)
    {
        return new DelegateIdGenerator<long>(seed, current => current + 1);
    }

    private sealed class DelegateIdGenerator<TKey> : IIdGenerator<TKey>
        where TKey : notnull
    {
        private readonly Func<TKey, TKey> _next;

        public DelegateIdGenerator(TKey initial, Func<TKey, TKey> next)
        {
            Initial = initial;
            _next = next;
        }

        public TKey Initial { get; }

        public TKey Next(TKey current)
        {
            return _next(current);
        }
    }
}
