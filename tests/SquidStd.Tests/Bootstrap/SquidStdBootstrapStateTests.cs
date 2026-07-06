using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Types.Bootstrap;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

public class SquidStdBootstrapStateTests
{
    [Fact]
    public async Task State_TransitionsAcrossLifecycle()
    {
        using var temp = new TempDirectory();
        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "statetest", RootDirectory = temp.Path });

        Assert.Equal(BootstrapStateType.Created, bootstrap.State);

        await bootstrap.StartAsync();
        Assert.Equal(BootstrapStateType.Started, bootstrap.State);

        await bootstrap.StopAsync();
        Assert.Equal(BootstrapStateType.Stopped, bootstrap.State);
    }
}
