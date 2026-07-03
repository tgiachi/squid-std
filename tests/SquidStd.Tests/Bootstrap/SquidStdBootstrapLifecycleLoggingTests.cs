using DryIoc;
using Serilog.Core;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Bootstrap;

[Collection(SerilogEventSinkCollection.Name)]
public class SquidStdBootstrapLifecycleLoggingTests
{
    [Fact]
    public async Task ConfigureLogging_AttachesContainerSinksAndEnrichesEvents()
    {
        using var temp = new TempDirectory();
        var sink = new CapturingLogSink();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path, AppName = "MyApp" }
        );
        bootstrap.ConfigureServices(c =>
        {
            c.RegisterInstance<ILogEventSink>(sink);
            return c;
        });

        bootstrap.ConfigureLogging();
        Serilog.Log.Information("probe");

        var probe = sink.Events.Single(e => e.MessageTemplate.Text == "probe");
        Assert.Equal("\"MyApp\"", probe.Properties["Application"].ToString());
        Assert.True(probe.Properties.ContainsKey("ApplicationVersion"));
    }

    [Fact]
    public async Task AppName_DefaultsToEntryAssemblyName()
    {
        using var temp = new TempDirectory();
        var sink = new CapturingLogSink();
        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = temp.Path }
        );
        bootstrap.ConfigureServices(c =>
        {
            c.RegisterInstance<ILogEventSink>(sink);
            return c;
        });

        bootstrap.ConfigureLogging();
        Serilog.Log.Information("probe");

        var probe = sink.Events.Single(e => e.MessageTemplate.Text == "probe");
        var application = probe.Properties["Application"].ToString().Trim('"');
        Assert.False(string.IsNullOrWhiteSpace(application));
        Assert.NotEqual("MyApp", application);
    }
}
