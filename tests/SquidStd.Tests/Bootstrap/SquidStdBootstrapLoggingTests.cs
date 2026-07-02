using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;
using SerilogILogger = Serilog.ILogger;

namespace SquidStd.Tests.Bootstrap;

[Collection(SerilogEventSinkCollection.Name)]
public class SquidStdBootstrapLoggingTests
{
    [Fact]
    public async Task ConfigureLogging_LoadsConfigurationAndRegistersLoggerEagerly()
    {
        using var temp = new TempDirectory();
        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path });

        bootstrap.ConfigureLogging();

        Assert.NotNull(bootstrap.Container.Resolve<SquidStdLoggerOptions>());
        Assert.NotNull(bootstrap.Container.Resolve<SerilogILogger>());
    }

    [Fact]
    public async Task ConfigureLogging_CalledTwice_ConfiguresLoggerOnce()
    {
        using var temp = new TempDirectory();
        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path });

        bootstrap.ConfigureLogging();
        var first = bootstrap.Container.Resolve<SerilogILogger>();

        bootstrap.ConfigureLogging();
        var second = bootstrap.Container.Resolve<SerilogILogger>();

        Assert.Same(first, second);
    }

    [Fact]
    public async Task ConfigureLogging_ThenStartAsync_DoesNotRebuildLogger()
    {
        using var temp = new TempDirectory();
        await using var bootstrap =
            SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path });

        bootstrap.ConfigureLogging();
        var eager = bootstrap.Container.Resolve<SerilogILogger>();

        await bootstrap.StartAsync();
        try
        {
            var afterStart = bootstrap.Container.Resolve<SerilogILogger>();
            Assert.Same(eager, afterStart);
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }
}
