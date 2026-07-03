using DryIoc;
using Serilog.Core;
using SquidStd.Abstractions.Data.Internal.Services;
using SquidStd.Abstractions.Extensions.Container;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Extensions;
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

    [Fact]
    public async Task StartAsync_LogsBannerRegistrationsAndServices()
    {
        using var temp = new TempDirectory();
        var (bootstrap, sink) = NewBootstrapWithSink(temp.Path, "MyApp");
        await using var _ = bootstrap;
        bootstrap.ConfigureServices(c => c.RegisterCoreServices());

        try
        {
            await bootstrap.StartAsync();

            Assert.True(HasMessage(sink, "MyApp"));
            Assert.True(HasMessage(sink, "starting (SquidStd"));
            Assert.True(HasMessage(sink, "Registered"));
            Assert.True(HasMessage(sink, "Started JobSystemService"));
            Assert.True(HasMessage(sink, "started:"));
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public async Task StartAsync_FailingService_LogsErrorWithServiceNameAndRethrows()
    {
        using var temp = new TempDirectory();
        var (bootstrap, sink) = NewBootstrapWithSink(temp.Path);
        await using var _ = bootstrap;
        var fake = new FakeLifecycleService { ThrowOnStart = true };
        bootstrap.ConfigureServices(c =>
        {
            c.RegisterInstance<FakeLifecycleService>(fake);
            c.AddToRegisterTypedList(
                new ServiceRegistrationData(typeof(FakeLifecycleService), typeof(FakeLifecycleService), 0)
            );
            return c;
        });

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await bootstrap.StartAsync());

        Assert.True(
            sink.Events.Any(e =>
            {
                var rendered = e.RenderMessage();

                return rendered.Contains("failed to start", StringComparison.Ordinal)
                       && rendered.Contains("FakeLifecycleService", StringComparison.Ordinal);
            })
        );
    }

    [Fact]
    public async Task StartAsync_ServiceBeforeConfigManager_IsStillLogged()
    {
        using var temp = new TempDirectory();
        var (bootstrap, sink) = NewBootstrapWithSink(temp.Path);
        await using var _ = bootstrap;
        var fake = new FakeLifecycleService();
        bootstrap.ConfigureServices(c =>
        {
            c.RegisterInstance<FakeLifecycleService>(fake);
            c.AddToRegisterTypedList(
                new ServiceRegistrationData(typeof(FakeLifecycleService), typeof(FakeLifecycleService), -2000)
            );
            return c;
        });

        try
        {
            await bootstrap.StartAsync();

            Assert.True(HasMessage(sink, "Started FakeLifecycleService"));
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    private static (SquidStdBootstrap Bootstrap, CapturingLogSink Sink) NewBootstrapWithSink(
        string root,
        string? appName = null
    )
    {
        var sink = new CapturingLogSink();
        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "app", RootDirectory = root, AppName = appName }
        );
        bootstrap.ConfigureServices(c =>
        {
            c.RegisterInstance<ILogEventSink>(sink);
            return c;
        });

        return (bootstrap, sink);
    }

    private static bool HasMessage(CapturingLogSink sink, string fragment)
        => sink.Events.Any(e => e.RenderMessage().Contains(fragment, StringComparison.Ordinal));
}
