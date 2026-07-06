using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Data.Config;
using SquidStd.Core.Data.EventLoop;
using SquidStd.Core.Data.Jobs;
using SquidStd.Core.Data.Metrics;
using SquidStd.Core.Data.Storage;
using SquidStd.Core.Data.Timing;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Services.Core;

public class ExplicitConfigRegistrationTests
{
    [Fact]
    public async Task RegisterEventLoop_ExplicitConfig_SurvivesStart_AndIgnoresFile()
    {
        using var root = new TempDirectory();
        File.WriteAllText(root.Combine("explicit.yaml"), "eventLoop:\n  IdleSleepMs: 99\n");
        var explicitConfig = new EventLoopConfig { IdleSleepMs = 3 };

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "explicit", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterMainThreadDispatcherService();
            c.RegisterTimerWheelService();
            c.RegisterEventLoop(explicitConfig);
            return c;
        });

        await bootstrap.StartAsync();

        Assert.Same(explicitConfig, bootstrap.Resolve<EventLoopConfig>());
        Assert.Equal(3, bootstrap.Resolve<EventLoopConfig>().IdleSleepMs);
        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task RegisterSchedulerServices_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new TimerWheelPumpConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "scheduler", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterSchedulerServices(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<TimerWheelPumpConfig>());
    }

    [Fact]
    public async Task RegisterTimerWheelService_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new TimerWheelConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "timerwheel", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterTimerWheelService(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<TimerWheelConfig>());
    }

    [Fact]
    public async Task RegisterJobSystemService_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new JobsConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "jobs", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterJobSystemService(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<JobsConfig>());
    }

    [Fact]
    public async Task RegisterMetricsCollectionService_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new MetricsConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "metrics", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterMetricsCollectionService(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<MetricsConfig>());
    }

    [Fact]
    public async Task RegisterSecretServices_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new SecretsConfig();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "secrets", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterSecretServices(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<SecretsConfig>());
    }

    [Fact]
    public async Task RegisterHealthChecksService_ExplicitConfig_IsResolved()
    {
        using var root = new TempDirectory();
        var explicitConfig = new HealthCheckOptions();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "health", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterHealthChecksService(explicitConfig);
            return c;
        });

        Assert.Same(explicitConfig, bootstrap.Resolve<HealthCheckOptions>());
    }
}
