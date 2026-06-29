using DryIoc;
using SquidStd.Crypto.Pgp.Extensions;
using SquidStd.Crypto.Pgp.Interfaces;
using SquidStd.Crypto.Pgp.Services;

namespace SquidStd.Tests.Crypto.Pgp;

public class RegisterPgpExtensionsTests
{
    [Fact]
    public void RegisterPgp_RegistersKeyringServiceAndStoreAsSingletons()
    {
        using var container = new Container();
        var dir = Path.Combine(Path.GetTempPath(), "squidstd-pgp-di-" + Guid.NewGuid().ToString("N"));

        container.RegisterPgp(_ => new FilePgpKeyStore(dir));

        var keyring = container.Resolve<IPgpKeyring>();
        var service = container.Resolve<IPgpService>();
        var store = container.Resolve<IPgpKeyStore>();

        Assert.Same(keyring, container.Resolve<IPgpKeyring>());
        Assert.NotNull(service);
        Assert.IsType<FilePgpKeyStore>(store);
    }
}
