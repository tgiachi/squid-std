using System.Text;
using SquidStd.Crypto.Pgp.Services;
using SquidStd.Services.Core.Services.Storage;
using SquidStd.Tests.Crypto.Pgp.Support;

namespace SquidStd.Tests.Crypto.Pgp;

[Collection("PgpKeys")]
public class AesGcmPgpKeyStoreTests
{
    private readonly PgpTestKeys _keys;

    public AesGcmPgpKeyStoreTests(PgpTestKeys keys)
    {
        _keys = keys;
    }

    [Fact]
    public async Task SaveThenLoad_RoundTripsAndBlobIsNotPlaintext()
    {
        var path = Path.Combine(Path.GetTempPath(), "squidstd-pgp-" + Guid.NewGuid().ToString("N") + ".bin");
        var protector = new AesGcmSecretProtector(new());

        try
        {
            var source = new PgpKeyring();
            source.Import(_keys.AlicePrivate);
            var store = new AesGcmPgpKeyStore(protector, path);
            await source.SaveAsync(store);

            var bytes = await File.ReadAllBytesAsync(path);
            var asText = Encoding.UTF8.GetString(bytes);
            Assert.DoesNotContain("BEGIN PGP", asText, StringComparison.Ordinal);

            var restored = new PgpKeyring();
            await restored.LoadAsync(store);
            Assert.True(restored.Find(PgpTestKeys.AliceIdentity)!.HasSecret);
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
