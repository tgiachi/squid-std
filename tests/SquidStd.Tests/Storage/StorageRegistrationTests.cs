using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Storage.Abstractions.Data.Config;
using SquidStd.Storage.Abstractions.Interfaces;
using SquidStd.Storage.Extensions;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Storage;

public class StorageRegistrationTests
{
    [Fact]
    public async Task AddFileStorage_ResolvesAndRoundTrips()
    {
        var root = Path.Combine(Path.GetTempPath(), "squidstd-storage-" + Guid.NewGuid().ToString("N"));
        using var container = new Container();

        container.AddFileStorage(new() { RootDirectory = root });

        var storage = container.Resolve<IStorageService>();
        Assert.NotNull(container.Resolve<IObjectStorageService>());

        await storage.SaveAsync("k", new byte[] { 1, 2, 3 });
        var loaded = await storage.LoadAsync("k");

        Assert.Equal(new byte[] { 1, 2, 3 }, loaded);

        Directory.Delete(root, true);
    }

    [Fact]
    public async Task AddFileStorage_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new StorageConfig { RootDirectory = root.Combine("data") };

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "storage", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.AddFileStorage(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<StorageConfig>());
    }
}
