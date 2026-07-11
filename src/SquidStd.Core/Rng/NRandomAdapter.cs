using SquidStd.Core.Interfaces.Rng;

namespace SquidStd.Core.Rng;

/// <summary>
/// Adapts an <see cref="NRandom.IRandom" /> generator to the SquidStd <see cref="IRandom" /> surface,
/// keeping consumer code independent of the underlying NRandom types.
/// </summary>
internal sealed class NRandomAdapter : IRandom
{
    private readonly NRandom.IRandom _inner;

    public NRandomAdapter(NRandom.IRandom inner)
    {
        _inner = inner;
    }

    public int NextInt()
    {
        return NRandom.RandomEx.NextInt(_inner);
    }

    public int NextInt(int maxExclusive)
    {
        return NRandom.RandomEx.NextInt(_inner, maxExclusive);
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        return NRandom.RandomEx.NextInt(_inner, minInclusive, maxExclusive);
    }

    public long NextLong()
    {
        return NRandom.RandomEx.NextLong(_inner);
    }

    public double NextDouble()
    {
        return NRandom.RandomEx.NextDouble(_inner);
    }

    public double NextDouble(double minInclusive, double maxExclusive)
    {
        return NRandom.RandomEx.NextDouble(_inner, minInclusive, maxExclusive);
    }

    public double NextGaussian(double mean = 0, double standardDeviation = 1)
    {
        return mean + (standardDeviation * NRandom.RandomEx.NextDoubleGaussian(_inner));
    }

    public bool NextBool(double probability = 0.5)
    {
        if (probability <= 0)
        {
            return false;
        }

        if (probability >= 1)
        {
            return true;
        }

        return NRandom.RandomEx.NextDouble(_inner) < probability;
    }

    public void NextBytes(Span<byte> buffer)
    {
        NRandom.RandomEx.NextBytes(_inner, buffer);
    }

    public T Pick<T>(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("Cannot pick from an empty collection.", nameof(items));
        }

        return items[NRandom.RandomEx.NextInt(_inner, items.Count)];
    }

    public void Shuffle<T>(IList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = NRandom.RandomEx.NextInt(_inner, i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
