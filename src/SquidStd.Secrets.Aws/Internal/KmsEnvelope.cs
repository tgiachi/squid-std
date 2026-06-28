using System.Buffers.Binary;
using System.Security.Cryptography;

namespace SquidStd.Secrets.Aws.Internal;

/// <summary>Frames an AES-GCM envelope: [int32 wrappedKeyLen | wrappedKey | nonce(12) | tag(16) | ciphertext].</summary>
internal static class KmsEnvelope
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static byte[] Seal(byte[] dataKey, byte[] wrappedKey, byte[] plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using (var aes = new AesGcm(dataKey, TagSize))
        {
            aes.Encrypt(nonce, plaintext, cipher, tag);
        }

        var output = new byte[4 + wrappedKey.Length + NonceSize + TagSize + cipher.Length];
        var span = output.AsSpan();
        BinaryPrimitives.WriteInt32BigEndian(span, wrappedKey.Length);
        wrappedKey.CopyTo(span[4..]);
        nonce.CopyTo(span[(4 + wrappedKey.Length)..]);
        tag.CopyTo(span[(4 + wrappedKey.Length + NonceSize)..]);
        cipher.CopyTo(span[(4 + wrappedKey.Length + NonceSize + TagSize)..]);

        return output;
    }

    public static byte[] ReadWrappedKey(byte[] blob)
    {
        var wrappedLen = BinaryPrimitives.ReadInt32BigEndian(blob);

        if (wrappedLen <= 0 || 4 + wrappedLen + NonceSize + TagSize > blob.Length)
        {
            throw new ArgumentException("Malformed KMS envelope.", nameof(blob));
        }

        return blob[4..(4 + wrappedLen)];
    }

    public static byte[] Open(byte[] dataKey, byte[] blob)
    {
        var wrappedLen = BinaryPrimitives.ReadInt32BigEndian(blob);
        var offset = 4 + wrappedLen;
        var nonce = blob.AsSpan(offset, NonceSize);
        var tag = blob.AsSpan(offset + NonceSize, TagSize);
        var cipher = blob.AsSpan(offset + NonceSize + TagSize);

        var plaintext = new byte[cipher.Length];
        using var aes = new AesGcm(dataKey, TagSize);
        aes.Decrypt(nonce, cipher, tag, plaintext);

        return plaintext;
    }
}
