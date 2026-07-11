using SquidStd.Core.Interfaces.Rng;
using SquidStd.Core.Types;

namespace SquidStd.Core.Rng;

/// <summary>
/// Entry point for obtaining <see cref="IRandom" /> instances. Consumers depend only on this factory
/// and <see cref="IRandom" />, never on the underlying NRandom generators.
/// </summary>
public static class RandomFactory
{
    private const int ChaChaRounds = 20;

    /// <summary>A shared, thread-safe random source seeded non-deterministically.</summary>
    public static IRandom Shared { get; } = new NRandomAdapter(NRandom.RandomEx.Shared);

    /// <summary>Creates a random source using the default algorithm and a non-deterministic seed.</summary>
    /// <returns>A new, non-thread-safe random source.</returns>
    public static IRandom Create()
    {
        return new NRandomAdapter(NRandom.RandomEx.Create());
    }

    /// <summary>Creates a reproducible random source using the default algorithm and the given seed.</summary>
    /// <param name="seed">The seed for reproducible sequences.</param>
    /// <returns>A new, non-thread-safe random source.</returns>
    public static IRandom Create(uint seed)
    {
        return Create(RandomAlgorithmType.Xoshiro256, seed);
    }

    /// <summary>Creates a random source using the given algorithm and a non-deterministic seed.</summary>
    /// <param name="algorithm">The pseudo-random algorithm to use.</param>
    /// <returns>A new, non-thread-safe random source.</returns>
    public static IRandom Create(RandomAlgorithmType algorithm)
    {
        return new NRandomAdapter(NewGenerator(algorithm));
    }

    /// <summary>Creates a reproducible random source using the given algorithm and seed.</summary>
    /// <param name="algorithm">The pseudo-random algorithm to use.</param>
    /// <param name="seed">The seed for reproducible sequences.</param>
    /// <returns>A new, non-thread-safe random source.</returns>
    public static IRandom Create(RandomAlgorithmType algorithm, uint seed)
    {
        var generator = NewGenerator(algorithm);
        generator.InitState(seed);

        return new NRandomAdapter(generator);
    }

    private static NRandom.IRandom NewGenerator(RandomAlgorithmType algorithm)
    {
        return algorithm switch
        {
            RandomAlgorithmType.Xoshiro256 => new NRandom.Xoshiro256StarStarRandom(),
            RandomAlgorithmType.Xoshiro128 => new NRandom.Xoshiro128StarStarRandom(),
            RandomAlgorithmType.Pcg32 => new NRandom.Pcg32Random(),
            RandomAlgorithmType.SplitMix64 => new NRandom.SplitMix64Random(),
            RandomAlgorithmType.MersenneTwister => new NRandom.MersenneTwisterRandom(),
            RandomAlgorithmType.ChaCha => new NRandom.ChaChaRandom(ChaChaRounds),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null),
        };
    }
}
