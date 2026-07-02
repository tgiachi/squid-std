using System.Text;
using SquidStd.Crypto.Vfs.Services;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.Extensions;
using SquidStd.Vfs.Services;

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions()
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory
    }
);

#region step-1

// Register a plain in-memory virtual filesystem.
bootstrap.ConfigureServices(container => container.RegisterVfs(_ => new InMemoryFileSystem()));

#endregion

await bootstrap.StartAsync();

#region step-2

// Write and read a file through the virtual filesystem.
var vfs = bootstrap.Resolve<IVirtualFileSystem>();

await vfs.WriteAllBytesAsync("notes/hello.txt", Encoding.UTF8.GetBytes("plain content"));
var bytes = await vfs.ReadAllBytesAsync("notes/hello.txt");

// The file was just written, so it is present (the API returns null only when absent).
Console.WriteLine($"VFS read: {Encoding.UTF8.GetString(bytes!)}");

#endregion

#region step-3

// Encrypted vault on a single on-disk zip file: unlock -> write -> dispose (flushes to disk),
// then re-open a brand-new instance over the same file to prove the data round-trips at rest.
var vaultPath = Path.Combine(Path.GetTempPath(), "squidstd-sample.vault");

using (var vault = new CryptoFileSystem(new ZipFileSystem(vaultPath)))
{
    vault.Unlock("vault passphrase");
    await vault.WriteAllBytesAsync("secret.txt", Encoding.UTF8.GetBytes("top secret"));
} // Dispose -> Lock (zeroes the key, flushes the encrypted index) -> flushes the zip to disk

// Re-open the same encrypted file with the passphrase; only the right passphrase decrypts it.
using (var reopened = new CryptoFileSystem(new ZipFileSystem(vaultPath)))
{
    reopened.Unlock("vault passphrase");

    var secret = await reopened.ReadAllBytesAsync("secret.txt");
    Console.WriteLine($"Vault read after reopen: {Encoding.UTF8.GetString(secret!)}");
}

File.Delete(vaultPath);

#endregion

await bootstrap.StopAsync();
