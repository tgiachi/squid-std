namespace SquidStd.Core.Types;

/// <summary>
/// Selects the pseudo-random algorithm backing an <see cref="Interfaces.Rng.IRandom" /> instance.
/// All options are non-cryptographic; use <see cref="Utils.CryptoUtils" /> or
/// <see cref="System.Security.Cryptography.RandomNumberGenerator" /> for security-sensitive randomness.
/// </summary>
public enum RandomAlgorithmType
{
    /// <summary>xoshiro256** — fast, high-quality general-purpose default.</summary>
    Xoshiro256,

    /// <summary>xoshiro128** — 32-bit oriented variant.</summary>
    Xoshiro128,

    /// <summary>PCG32 (PCG-XSH-RR) — compact state with strong statistical quality.</summary>
    Pcg32,

    /// <summary>SplitMix64 — minimal state, well suited to seeding other generators.</summary>
    SplitMix64,

    /// <summary>Mersenne Twister (MT19937) — very long period, classic choice.</summary>
    MersenneTwister,

    /// <summary>ChaCha (20 rounds) — highest statistical quality of the set, but slower.</summary>
    ChaCha,
}
