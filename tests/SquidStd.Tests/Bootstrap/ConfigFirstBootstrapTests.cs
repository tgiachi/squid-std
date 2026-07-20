using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Config;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Core.Types;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

public class ConfigFirstBootstrapTests
{
    public sealed class HostSection
    {
        public string Mode { get; set; } = "default";
    }

    [Fact]
    public async Task Create_WithExplicitConfig_SectionsBindAtRegistration()
    {
        using var root = new TempDirectory();
        File.WriteAllText(root.Combine("app.yaml"), "host:\n  Mode: fromfile\n");
        var config = SquidStdConfig.Load("app", root.Path);

        await using var bootstrap =
            SquidStdBootstrap.Create(config, new SquidStdOptions { ConfigName = "app", RootDirectory = root.Path });

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterConfigSection("host", static () => new HostSection());
            return c;
        });

        // bound IMMEDIATELY, before any StartAsync
        Assert.Equal("fromfile", bootstrap.Resolve<HostSection>().Mode);
    }

    [Fact]
    public async Task StartAsync_ExistingPrimary_GeneratesMissingExternalConfigFile()
    {
        using var root = new TempDirectory();
        using var external = new TempDirectory();
        File.WriteAllText(root.Combine("app.yaml"), "host:\n  Mode: fromfile\n"); // primary already exists

        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "app", RootDirectory = root.Path });

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterConfigFile<HostSection>("host", external.Path);
            return c;
        });

        await bootstrap.StartAsync();

        Assert.True(File.Exists(Path.Combine(external.Path, "host.yaml"))); // generated despite the primary existing

        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task OnConfigLoaded_AppliesOnceAtStart_AndSticks()
    {
        using var root = new TempDirectory();
        var applied = 0;

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "hooktest", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterConfigSection("host", static () => new HostSection());
            return c;
        });
        bootstrap.OnConfigLoaded<HostSection>(section =>
        {
            applied++;
            section.Mode = "hooked";
        });

        await bootstrap.StartAsync();

        Assert.Equal(1, applied);
        Assert.Equal("hooked", bootstrap.Resolve<HostSection>().Mode);
        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task StartAsync_MissingFile_WritesDefaults()
    {
        using var root = new TempDirectory();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "writedefaults", RootDirectory = root.Path }
        );

        await bootstrap.StartAsync();

        Assert.True(File.Exists(root.Combine("writedefaults.yaml")));
        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task ExplicitLoad_Reloads_FiresEvent_AndReappliesHooks()
    {
        using var root = new TempDirectory();
        File.WriteAllText(root.Combine("reload.yaml"), "host:\n  Mode: v1\n");
        var hookRuns = 0;

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "reload", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterConfigSection("host", static () => new HostSection());
            return c;
        });
        bootstrap.OnConfigLoaded<HostSection>(_ => hookRuns++);

        await bootstrap.StartAsync();
        Assert.Equal(1, hookRuns);

        File.WriteAllText(root.Combine("reload.yaml"), "host:\n  Mode: v2\n");
        var manager = bootstrap.Resolve<IConfigManagerService>();
        manager.Load();

        Assert.Equal(2, hookRuns);
        Assert.Equal("v2", bootstrap.Resolve<HostSection>().Mode);
        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task OptionsLogger_BypassesTheFileSection()
    {
        using var root = new TempDirectory();
        File.WriteAllText(root.Combine("logopt.yaml"), "logger:\n  MinimumLevel: Error\n");
        var explicitLogger = new SquidStdLoggerOptions { MinimumLevel = LogLevelType.Debug };

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "logopt", RootDirectory = root.Path, Logger = explicitLogger }
        );

        await bootstrap.StartAsync();

        Assert.Same(explicitLogger, bootstrap.Resolve<SquidStdLoggerOptions>());
        Assert.Equal(LogLevelType.Debug, bootstrap.Resolve<SquidStdLoggerOptions>().MinimumLevel);
        await bootstrap.StopAsync();
    }
}
