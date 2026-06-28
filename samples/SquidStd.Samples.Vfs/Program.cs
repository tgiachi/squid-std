using System.Text;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Crypto.Vfs.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.Extensions;
using SquidStd.Vfs.Services;

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory
    }
);

var vaultPath = Path.Combine(AppContext.BaseDirectory, "secret.vault");

#region step-1

// Register a plain in-memory VFS and an encrypted single-file vault.
bootstrap.ConfigureServices(container =>
{
    container.RegisterVfs(_ => new InMemoryFileSystem());
    container.RegisterCryptoVault(vaultPath);

    return container;
});

#endregion

await bootstrap.StartAsync();

#region step-2

// Write and read a file through the virtual filesystem.
var vfs = bootstrap.Resolve<IVirtualFileSystem>();

await vfs.WriteAllBytesAsync("notes/hello.txt", Encoding.UTF8.GetBytes("plain content"));
var bytes = await vfs.ReadAllBytesAsync("notes/hello.txt");

Console.WriteLine($"VFS read: {Encoding.UTF8.GetString(bytes!)}");

#endregion

#region step-3

// Unlock the encrypted vault, write a secret, then lock it again.
var vault = bootstrap.Resolve<ILockableFileSystem>();

vault.Unlock("vault passphrase");
await vault.WriteAllBytesAsync("secret.txt", Encoding.UTF8.GetBytes("top secret"));
var secret = await vault.ReadAllBytesAsync("secret.txt");
Console.WriteLine($"Vault read: {Encoding.UTF8.GetString(secret!)}");
vault.Lock();

Console.WriteLine($"Vault unlocked: {vault.IsUnlocked}");

#endregion

await bootstrap.StopAsync();
