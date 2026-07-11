namespace SquidStd.Core.Interfaces.Rng;

/// <summary>
/// A source of non-cryptographic pseudo-random values. Obtain instances from
/// <see cref="SquidStd.Core.Rng.RandomFactory" />; instances created with a fixed seed produce
/// reproducible sequences.
/// </summary>
public interface IRandom
{
    /// <summary>Returns a random 32-bit integer across the full range (may be negative).</summary>
    int NextInt();

    /// <summary>Returns a random integer in <c>[0, maxExclusive)</c>.</summary>
    /// <param name="maxExclusive">Exclusive upper bound; must be positive.</param>
    int NextInt(int maxExclusive);

    /// <summary>Returns a random integer in <c>[minInclusive, maxExclusive)</c>.</summary>
    /// <param name="minInclusive">Inclusive lower bound.</param>
    /// <param name="maxExclusive">Exclusive upper bound; must exceed <paramref name="minInclusive" />.</param>
    int NextInt(int minInclusive, int maxExclusive);

    /// <summary>Returns a random 64-bit integer.</summary>
    long NextLong();

    /// <summary>Returns a random double in <c>[0, 1)</c>.</summary>
    double NextDouble();

    /// <summary>Returns a random double in <c>[minInclusive, maxExclusive)</c>.</summary>
    /// <param name="minInclusive">Inclusive lower bound.</param>
    /// <param name="maxExclusive">Exclusive upper bound.</param>
    double NextDouble(double minInclusive, double maxExclusive);

    /// <summary>Returns a normally-distributed random double.</summary>
    /// <param name="mean">The distribution mean.</param>
    /// <param name="standardDeviation">The distribution standard deviation.</param>
    double NextGaussian(double mean = 0, double standardDeviation = 1);

    /// <summary>Returns <c>true</c> with the given probability.</summary>
    /// <param name="probability">Chance of <c>true</c> in <c>[0, 1]</c>; defaults to 0.5.</param>
    bool NextBool(double probability = 0.5);

    /// <summary>Fills the buffer with random bytes.</summary>
    /// <param name="buffer">The buffer to fill.</param>
    void NextBytes(Span<byte> buffer);

    /// <summary>Returns a uniformly-chosen element from a non-empty list.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The list to pick from; must be non-empty.</param>
    T Pick<T>(IReadOnlyList<T> items);

    /// <summary>Shuffles the list in place using the Fisher–Yates algorithm.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The list to shuffle.</param>
    void Shuffle<T>(IList<T> items);
}
