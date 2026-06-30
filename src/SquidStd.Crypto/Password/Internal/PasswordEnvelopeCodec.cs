using System.Buffers.Binary;

namespace SquidStd.Crypto.Password.Internal;

/// <summary>Encodes/decodes the self-describing <c>SPBE</c> password-encryption envelope.</summary>
internal static class PasswordEnvelopeCodec
{
    public const byte Version = 1;
    public const int SaltSize = 16;
    public const int NonceSize = 12;
    public const int TagSize = 16;

    // magic(4) | version(1) | iterations(4) | memoryKib(4) | parallelism(1) | salt(16) | nonce(12)
    public const int AadSize = 4 + 1 + 4 + 4 + 1 + SaltSize + NonceSize; // 42 — authenticated, excludes the tag
    public const int HeaderSize = AadSize + TagSize;                     // 58 — everything before the ciphertext

    private static readonly byte[] _magic = "SPBE"u8.ToArray();

    public static byte[] BuildAad(
        byte version, int iterations, int memoryKib, int parallelism, ReadOnlySpan<byte> salt, ReadOnlySpan<byte> nonce
    )
    {
        var aad = new byte[AadSize];
        var span = aad.AsSpan();

        _magic.CopyTo(span);
        span[4] = version;
        BinaryPrimitives.WriteInt32BigEndian(span.Slice(5, 4), iterations);
        BinaryPrimitives.WriteInt32BigEndian(span.Slice(9, 4), memoryKib);
        span[13] = (byte)parallelism;
        salt.CopyTo(span.Slice(14, SaltSize));
        nonce.CopyTo(span.Slice(30, NonceSize));

        return aad;
    }

    public static byte[] Encode(PasswordEnvelope envelope)
    {
        var aad = BuildAad(
            envelope.Version, envelope.Iterations, envelope.MemoryKib, envelope.Parallelism, envelope.Salt, envelope.Nonce
        );

        var result = new byte[HeaderSize + envelope.Ciphertext.Length];
        aad.CopyTo(result.AsSpan());
        envelope.Tag.CopyTo(result.AsSpan(AadSize, TagSize));
        envelope.Ciphertext.CopyTo(result.AsSpan(HeaderSize));

        return result;
    }

    public static PasswordEnvelope Decode(byte[] blob)
    {
        ArgumentNullException.ThrowIfNull(blob);

        if (blob.Length < HeaderSize)
        {
            throw new InvalidDataException("Encrypted payload is too short.");
        }

        var span = blob.AsSpan();

        if (!span[..4].SequenceEqual(_magic))
        {
            throw new InvalidDataException("Encrypted payload has an invalid magic header.");
        }

        var version = span[4];

        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported encrypted payload version {version}.");
        }

        var iterations = BinaryPrimitives.ReadInt32BigEndian(span.Slice(5, 4));
        var memoryKib = BinaryPrimitives.ReadInt32BigEndian(span.Slice(9, 4));
        var parallelism = span[13];

        if (iterations <= 0 || memoryKib <= 0 || parallelism <= 0)
        {
            throw new InvalidDataException("Encrypted payload has invalid KDF parameters.");
        }

        var salt = span.Slice(14, SaltSize).ToArray();
        var nonce = span.Slice(30, NonceSize).ToArray();
        var tag = span.Slice(AadSize, TagSize).ToArray();
        var ciphertext = span[HeaderSize..].ToArray();

        return new PasswordEnvelope(version, iterations, memoryKib, parallelism, salt, nonce, tag, ciphertext);
    }

    public static ReadOnlySpan<byte> AadOf(byte[] blob)
    {
        return blob.AsSpan(0, AadSize);
    }
}
