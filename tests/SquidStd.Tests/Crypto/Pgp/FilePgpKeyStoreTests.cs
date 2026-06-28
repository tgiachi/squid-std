using SquidStd.Crypto.Pgp.Services;
using SquidStd.Tests.Crypto.Pgp.Support;

namespace SquidStd.Tests.Crypto.Pgp;

[Collection("PgpKeys")]
public class FilePgpKeyStoreTests
{
    private readonly PgpTestKeys _keys;

    public FilePgpKeyStoreTests(PgpTestKeys keys)
    {
        _keys = keys;
    }

    [Fact]
    public async Task SaveThenLoad_RestoresPublicAndSecretKeys()
    {
        var dir = Path.Combine(Path.GetTempPath(), "squidstd-pgp-" + Guid.NewGuid().ToString("N"));

        try
        {
            var source = new PgpKeyring();
            source.Import(_keys.AlicePrivate); // has secret
            source.Import(_keys.BobPublic);    // public only

            var store = new FilePgpKeyStore(dir);
            await source.SaveAsync(store);

            var restored = new PgpKeyring();
            await restored.LoadAsync(store);

            Assert.Equal(2, restored.Keys.Count);
            Assert.True(restored.Find(PgpTestKeys.AliceIdentity)!.HasSecret);
            Assert.False(restored.Find(PgpTestKeys.BobIdentity)!.HasSecret);
        }
        finally
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
