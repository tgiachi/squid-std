using System.Text;
using SquidStd.Crypto.Vfs.Data;
using SquidStd.Crypto.Vfs.Services;
using SquidStd.Vfs.Services;

namespace SquidStd.Tests.Crypto.Vfs;

public class CryptoFileSystemDiskTests
{
    private static CryptoVaultOptions FastOptions()
    {
        return new CryptoVaultOptions { Argon2MemoryKib = 8192, Argon2Iterations = 1 };
    }

    [Fact]
    public async Task Lock_AfterWrites_OverZipBackend_DoesNotThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), "squidstd-vault-" + Guid.NewGuid().ToString("N") + ".vault");

        try
        {
            var vault = new CryptoFileSystem(new ZipFileSystem(path), FastOptions());
            vault.Unlock("pw");
            await vault.WriteAllBytesAsync("secret.txt", Encoding.UTF8.GetBytes("top secret"));

            // PruneOrphans() lists the zip backend, which previously threw because ZipArchiveEntry.Length
            // is unavailable in update mode after a same-session write.
            vault.Lock();

            Assert.False(vault.IsUnlocked);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task Vault_PersistsToDisk_AcrossInstances()
    {
        var path = Path.Combine(Path.GetTempPath(), "squidstd-vault-" + Guid.NewGuid().ToString("N") + ".vault");

        try
        {
            using (var vault = new CryptoFileSystem(new ZipFileSystem(path), FastOptions()))
            {
                vault.Unlock("correct horse");
                await vault.WriteAllBytesAsync("notes/plan.txt", Encoding.UTF8.GetBytes("attack at dawn"));
            } // Dispose -> Lock -> flush the inner zip to disk

            Assert.True(new FileInfo(path).Length > 0, "the vault file must be written to disk");

            using (var reopened = new CryptoFileSystem(new ZipFileSystem(path), FastOptions()))
            {
                reopened.Unlock("correct horse");
                var data = await reopened.ReadAllBytesAsync("notes/plan.txt");

                Assert.NotNull(data);
                Assert.Equal("attack at dawn", Encoding.UTF8.GetString(data!));
            }
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
