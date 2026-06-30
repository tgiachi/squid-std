using SquidStd.Crypto.Password.Internal;

namespace SquidStd.Tests.Crypto.Password;

public class PasswordEnvelopeCodecTests
{
    private static PasswordEnvelope Sample()
    {
        return new PasswordEnvelope(
            PasswordEnvelopeCodec.Version,
            iterations: 3,
            memoryKib: 65536,
            parallelism: 1,
            salt: Enumerable.Range(0, PasswordEnvelopeCodec.SaltSize).Select(i => (byte)i).ToArray(),
            nonce: Enumerable.Range(0, PasswordEnvelopeCodec.NonceSize).Select(i => (byte)(i + 100)).ToArray(),
            tag: Enumerable.Range(0, PasswordEnvelopeCodec.TagSize).Select(i => (byte)(i + 200)).ToArray(),
            ciphertext: [1, 2, 3, 4, 5]);
    }

    [Fact]
    public void Encode_Decode_RoundTripsAllFields()
    {
        var original = Sample();

        var decoded = PasswordEnvelopeCodec.Decode(PasswordEnvelopeCodec.Encode(original));

        Assert.Equal(original.Version, decoded.Version);
        Assert.Equal(original.Iterations, decoded.Iterations);
        Assert.Equal(original.MemoryKib, decoded.MemoryKib);
        Assert.Equal(original.Parallelism, decoded.Parallelism);
        Assert.Equal(original.Salt, decoded.Salt);
        Assert.Equal(original.Nonce, decoded.Nonce);
        Assert.Equal(original.Tag, decoded.Tag);
        Assert.Equal(original.Ciphertext, decoded.Ciphertext);
    }

    [Fact]
    public void AadOf_MatchesEverythingBeforeTheTag()
    {
        var blob = PasswordEnvelopeCodec.Encode(Sample());
        var aad = PasswordEnvelopeCodec.AadOf(blob);

        Assert.Equal(PasswordEnvelopeCodec.AadSize, aad.Length);
        Assert.True(aad.SequenceEqual(blob.AsSpan(0, PasswordEnvelopeCodec.AadSize)));
    }

    [Fact]
    public void Decode_TooShort_Throws()
        => Assert.Throws<InvalidDataException>(() => PasswordEnvelopeCodec.Decode(new byte[10]));

    [Fact]
    public void Decode_BadMagic_Throws()
    {
        var blob = PasswordEnvelopeCodec.Encode(Sample());
        blob[0] ^= 0xFF;

        Assert.Throws<InvalidDataException>(() => PasswordEnvelopeCodec.Decode(blob));
    }

    [Fact]
    public void Decode_UnknownVersion_Throws()
    {
        var blob = PasswordEnvelopeCodec.Encode(Sample());
        blob[4] = 99;

        Assert.Throws<InvalidDataException>(() => PasswordEnvelopeCodec.Decode(blob));
    }
}
