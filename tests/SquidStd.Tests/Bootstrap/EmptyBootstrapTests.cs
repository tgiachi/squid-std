using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Core.Interfaces.Jobs;
using SquidStd.Core.Interfaces.Threading;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

[Collection(SerilogEventSinkCollection.Name)]
public class EmptyBootstrapTests
{
    [Fact]
    public async Task Create_RegistersOnlyConfigCore()
    {
        using var temp = new TempDirectory();
        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "squidstd", RootDirectory = temp.Path });

        Assert.True(bootstrap.Container.IsRegistered<IConfigManagerService>());
        Assert.False(bootstrap.Container.IsRegistered<IEventBus>());
        Assert.False(bootstrap.Container.IsRegistered<IJobSystem>());
        Assert.False(bootstrap.Container.IsRegistered<ITimerService>());
        Assert.False(bootstrap.Container.IsRegistered<IMainThreadDispatcher>());
    }

    [Fact]
    public async Task RegisterCoreServices_AfterCreate_CompletesTheSetAndStarts()
    {
        using var temp = new TempDirectory();
        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "squidstd", RootDirectory = temp.Path });

        bootstrap.ConfigureServices(c => c.RegisterCoreServices());

        await bootstrap.StartAsync();
        try
        {
            Assert.NotNull(bootstrap.Resolve<IEventBus>());
            Assert.NotNull(bootstrap.Resolve<ITimerService>());
            Assert.NotNull(bootstrap.Resolve<IMainThreadDispatcher>());
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public async Task EmptyBootstrap_StartsAndConfiguresLogging()
    {
        using var temp = new TempDirectory();
        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "squidstd", RootDirectory = temp.Path });

        await bootstrap.StartAsync();
        try
        {
            Assert.NotNull(bootstrap.Container.Resolve<SquidStdLoggerOptions>());
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public void RegisterCoreServices_WithNameAndDir_StandaloneStillRegistersEverything()
    {
        using var temp = new TempDirectory();
        using var container = new Container();

        container.RegisterCoreServices("squidstd", temp.Path);

        Assert.True(container.IsRegistered<IConfigManagerService>());
        Assert.True(container.IsRegistered<IEventBus>());
        Assert.True(container.IsRegistered<IJobSystem>());
        Assert.True(container.IsRegistered<ITimerService>());
        Assert.True(container.IsRegistered<IMainThreadDispatcher>());
    }

    [Fact]
    public async Task Create_WithConfigureAction_AppliesOptions()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = SquidStdBootstrap.Create(
            o =>
            {
                o.ConfigName = "myapp";
                o.RootDirectory = temp.Path;
            }
        );

        Assert.Equal("myapp", bootstrap.Options.ConfigName);
        Assert.Equal(temp.Path, bootstrap.Options.RootDirectory);
    }
}
