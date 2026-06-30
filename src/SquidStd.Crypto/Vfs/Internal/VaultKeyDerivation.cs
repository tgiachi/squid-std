using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using SquidStd.Crypto.Vfs.Data;

namespace SquidStd.Crypto.Vfs.Internal;

/// <summary>Derives the vault master key (Argon2id) and per-purpose subkeys (HKDF-SHA256).</summary>
internal static class VaultKeyDerivation
{
    public static byte[] DeriveMasterKey(string passphrase, byte[] salt, CryptoVaultOptions options)
    {
        var parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                         .WithVersion(Argon2Parameters.Version13)
                         .WithSalt(salt)
                         .WithIterations(options.Argon2Iterations)
                         .WithMemoryAsKB(options.Argon2MemoryKib)
                         .WithParallelism(options.Argon2Parallelism)
                         .Build();

        var generator = new Argon2BytesGenerator();
        generator.Init(parameters);

        var key = new byte[32];
        generator.GenerateBytes(Encoding.UTF8.GetBytes(passphrase), key);

        return key;
    }

    public static byte[] DeriveSubKey(byte[] masterKey, string label)
    {
        var hkdf = new HkdfBytesGenerator(new Sha256Digest());
        hkdf.Init(new HkdfParameters(masterKey, null, Encoding.UTF8.GetBytes(label)));

        var subKey = new byte[32];
        hkdf.GenerateBytes(subKey, 0, subKey.Length);

        return subKey;
    }
}
