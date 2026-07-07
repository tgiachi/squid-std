using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

public class BootstrapDirectoriesTests
{
    [Fact]
    public async Task Create_WithDeclaredDirectories_CreatesThemImmediately()
    {
        using var root = new TempDirectory();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions
            {
                ConfigName = "dirs",
                RootDirectory = root.Path,
                Directories = ["scripts", "save"]
            }
        );

        // created at Create time, BEFORE any StartAsync
        Assert.True(Directory.Exists(root.Combine("scripts")));
        Assert.True(Directory.Exists(root.Combine("save")));
    }
}
