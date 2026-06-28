using System.Text;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Crypto.Pgp.Extensions;
using SquidStd.Crypto.Pgp.Interfaces;
using SquidStd.Crypto.Pgp.Services;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory
    }
);

var keyStoreDirectory = Path.Combine(AppContext.BaseDirectory, "pgp-keys");

#region step-1

// Register the PGP keyring, service, and a file-backed key store (armored .asc files).
bootstrap.ConfigureServices(container => container.RegisterPgp(_ => new FilePgpKeyStore(keyStoreDirectory)));

#endregion

await bootstrap.StartAsync();

#region step-2

const string identity = "alice@example.com";
const string passphrase = "correct horse battery staple";

var pgp = bootstrap.Resolve<IPgpService>();
var keyring = bootstrap.Resolve<IPgpKeyring>();

// Generate a key pair and import it so the service can resolve it by identity.
var key = pgp.GenerateKey(identity, passphrase);
keyring.Import(key.PrivateArmored!);

Console.WriteLine($"Generated key {key.KeyId} for {key.Identity} (has secret: {key.HasSecret})");

#endregion

#region step-3

// Encrypt + sign for the recipient, then decrypt + verify the round-trip.
var armored = await pgp.EncryptAndSignForAsync(
    identity,
    Encoding.UTF8.GetBytes("attack at dawn"),
    identity,
    passphrase
);

var result = await pgp.DecryptAndVerifyAsync(armored, passphrase);

Console.WriteLine(
    $"Decrypted: '{Encoding.UTF8.GetString(result.Data)}' (signed: {result.IsSigned}, valid: {result.IsValid})"
);

#endregion

await bootstrap.StopAsync();
