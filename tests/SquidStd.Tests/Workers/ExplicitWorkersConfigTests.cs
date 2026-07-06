using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Data.Timing;
using SquidStd.Messaging.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;
using SquidStd.Workers.Data.Config;
using SquidStd.Workers.Extensions;
using SquidStd.Workers.Manager.Data.Config;
using SquidStd.Workers.Manager.Extensions;

namespace SquidStd.Tests.Workers;

public class ExplicitWorkersConfigTests
{
    [Fact]
    public async Task AddWorkers_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new WorkersConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "workers", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.AddWorkers(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<WorkersConfig>());
    }

    [Fact]
    public async Task AddWorkerManager_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new WorkerManagerConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "workermanager", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.AddWorkerManager(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<WorkerManagerConfig>());
    }

    [Fact]
    public async Task ExplicitPumpConfig_SurvivesWorkerManagerAutoRegistration()
    {
        using var root = new TempDirectory();
        var pump = new TimerWheelPumpConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "pumpguard", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterCoreServices();
            c.AddInMemoryMessaging();
            c.RegisterSchedulerServices(pump);
            c.AddWorkerManager(); // auto-registers the timerWheelPump section
            return c;
        });

        await bootstrap.StartAsync();

        Assert.Same(pump, bootstrap.Resolve<TimerWheelPumpConfig>());
        await bootstrap.StopAsync();
    }
}
