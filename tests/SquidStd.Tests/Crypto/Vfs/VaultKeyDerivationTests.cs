using SquidStd.Crypto.Vfs.Data;
using SquidStd.Crypto.Vfs.Internal;

namespace SquidStd.Tests.Crypto.Vfs;

public class VaultKeyDerivationTests
{
    [Fact]
    public void DeriveMasterKey_IsDeterministicForSameSaltAndPassphrase()
    {
        var options = new CryptoVaultOptions { Argon2MemoryKib = 8192, Argon2Iterations = 1 };
        var salt = new byte[16];

        var a = VaultKeyDerivation.DeriveMasterKey("pw", salt, options);
        var b = VaultKeyDerivation.DeriveMasterKey("pw", salt, options);
        var c = VaultKeyDerivation.DeriveMasterKey("other", salt, options);

        Assert.Equal(32, a.Length);
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void DeriveSubKey_DiffersByLabel()
    {
        var master = new byte[32];
        Assert.NotEqual(
            VaultKeyDerivation.DeriveSubKey(master, "index"),
            VaultKeyDerivation.DeriveSubKey(master, "entry:a1b2")
        );
    }
}
