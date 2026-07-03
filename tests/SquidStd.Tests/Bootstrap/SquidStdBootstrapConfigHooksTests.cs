using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Core.Types;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

[Collection(SerilogEventSinkCollection.Name)]
public class SquidStdBootstrapConfigHooksTests
{
    [Fact]
    public async Task OnConfigLoaded_MutatesSection_VisibleAfterStart()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);

        bootstrap.ConfigureServices(
            container => container.RegisterConfigSection("fakeSection", static () => new FakeSectionConfig(), 0)
        );
        bootstrap.OnConfigLoaded<FakeSectionConfig>(fake => fake.Limit = 42);

        await bootstrap.StartAsync(CancellationToken.None);

        try
        {
            var config = bootstrap.Resolve<IConfigManagerService>().GetConfig<FakeSectionConfig>();

            Assert.Equal(42, config.Limit);
        }
        finally
        {
            await bootstrap.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task OnConfigLoaded_SurvivesTheSecondLoad()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);
        var invocations = 0;

        bootstrap.ConfigureServices(
            container => container.RegisterConfigSection("fakeSection", static () => new FakeSectionConfig(), 0)
        );
        bootstrap.OnConfigLoaded<FakeSectionConfig>(
            fake =>
            {
                invocations++;
                fake.Limit = 42;
            }
        );

        await bootstrap.StartAsync(CancellationToken.None);

        try
        {
            var config = bootstrap.Resolve<IConfigManagerService>().GetConfig<FakeSectionConfig>();

            Assert.True(invocations >= 2, $"Expected the hook to run on every load, but it ran {invocations} time(s).");
            Assert.Equal(42, config.Limit);
        }
        finally
        {
            await bootstrap.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task OnConfigLoaded_LoggerSection_AffectsLoggerConfiguration()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);

        bootstrap.OnConfigLoaded<SquidStdLoggerOptions>(options => options.MinimumLevel = LogLevelType.None);

        bootstrap.ConfigureLogging();

        var config = bootstrap.Resolve<IConfigManagerService>().GetConfig<SquidStdLoggerOptions>();

        Assert.Equal(LogLevelType.None, config.MinimumLevel);
    }

    [Fact]
    public async Task OnConfigLoaded_MultipleHooksSameType_RunInOrder()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);

        bootstrap.ConfigureServices(
            container => container.RegisterConfigSection("fakeSection", static () => new FakeSectionConfig(), 0)
        );
        bootstrap.OnConfigLoaded<FakeSectionConfig>(fake => fake.Name = "first");
        bootstrap.OnConfigLoaded<FakeSectionConfig>(fake => fake.Name += "+second");

        await bootstrap.StartAsync(CancellationToken.None);

        try
        {
            var config = bootstrap.Resolve<IConfigManagerService>().GetConfig<FakeSectionConfig>();

            Assert.Equal("first+second", config.Name);
        }
        finally
        {
            await bootstrap.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task OnConfigLoaded_UnregisteredType_Throws()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);

        bootstrap.OnConfigLoaded<FakeSectionConfig>(fake => fake.Limit = 42);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => bootstrap.StartAsync(CancellationToken.None).AsTask()
        );

        Assert.Contains(nameof(FakeSectionConfig), exception.Message);
        Assert.Contains("config section", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OnConfigLoaded_AfterStart_Throws()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);

        await bootstrap.StartAsync(CancellationToken.None);

        try
        {
            Assert.Throws<InvalidOperationException>(
                () => bootstrap.OnConfigLoaded<FakeSectionConfig>(fake => fake.Limit = 42)
            );
        }
        finally
        {
            await bootstrap.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task OnConfigLoaded_DoesNotRewriteYamlFile()
    {
        using var temp = new TempDirectory();
        File.WriteAllText(
            temp.Combine("app.yaml"),
            "fakeSection:\n  Name: fromfile\n  Limit: 7\n"
        );
        await using var bootstrap = NewBootstrap(temp.Path);

        bootstrap.ConfigureServices(
            container => container.RegisterConfigSection("fakeSection", static () => new FakeSectionConfig(), 0)
        );
        bootstrap.OnConfigLoaded<FakeSectionConfig>(fake => fake.Limit = 999);

        await bootstrap.StartAsync(CancellationToken.None);

        try
        {
            var config = bootstrap.Resolve<IConfigManagerService>().GetConfig<FakeSectionConfig>();
            var fileContent = File.ReadAllText(temp.Combine("app.yaml"));

            Assert.Equal(999, config.Limit);
            Assert.Equal("fromfile", config.Name);
            Assert.Contains("Limit: 7", fileContent);
            Assert.DoesNotContain("999", fileContent);
        }
        finally
        {
            await bootstrap.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task OnConfigReady_ReceivesManagerWithHookAdjustedValues()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);
        var observedLimit = 0;
        var invocations = 0;
        bootstrap.ConfigureServices(c =>
        {
            c.RegisterConfigSection("fakeSection", static () => new FakeSectionConfig(), 0);
            return c;
        });
        bootstrap.OnConfigLoaded<FakeSectionConfig>(f => f.Limit = 42);
        bootstrap.OnConfigReady(cfg =>
        {
            invocations++;
            observedLimit = cfg.GetConfig<FakeSectionConfig>().Limit;
        });

        await bootstrap.StartAsync();
        try
        {
            Assert.True(invocations >= 1);
            Assert.Equal(42, observedLimit);
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public async Task OnConfigReady_MultipleCallbacks_RunInOrder()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);
        var order = new List<string>();
        bootstrap.OnConfigReady(_ => order.Add("a"));
        bootstrap.OnConfigReady(_ => order.Add("b"));

        await bootstrap.StartAsync();
        try
        {
            Assert.True(order.Count >= 2);
            Assert.Equal("a", order[0]);
            Assert.Equal("b", order[1]);
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public async Task OnConfigReady_WithoutTypedHooks_StillRuns()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);
        var composed = string.Empty;
        bootstrap.OnConfigReady(cfg => composed = cfg.Compose());

        await bootstrap.StartAsync();
        try
        {
            Assert.Contains("logger", composed, StringComparison.Ordinal);
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public async Task OnConfigReady_AfterStart_Throws()
    {
        using var temp = new TempDirectory();
        await using var bootstrap = NewBootstrap(temp.Path);

        await bootstrap.StartAsync();
        try
        {
            Assert.Throws<InvalidOperationException>(
                () => bootstrap.OnConfigReady(_ => { })
            );
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    private static SquidStdBootstrap NewBootstrap(string root)
        => SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "app", RootDirectory = root });
}
