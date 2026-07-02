using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using SquidStd.AspNetCore.Extensions;
using SquidStd.Tests.Support;
using SerilogLog = Serilog.Log;

namespace SquidStd.Tests.AspNetCore;

[Collection(SerilogEventSinkCollection.Name)]
public class SquidStdSerilogExtensionsTests
{
    [Fact]
    public async Task AddSquidStdSerilog_RoutesFrameworkLoggingThroughSerilog()
    {
        using var temp = new TempDirectory();
        var builder = CreateBuilder(temp.Path);
        builder.UseSquidStd(options => options.ConfigName = "app");
        builder.AddSquidStdSerilog();

        await using var app = builder.Build();

        var factory = app.Services.GetRequiredService<ILoggerFactory>();
        Assert.IsType<SerilogLoggerFactory>(factory);
    }

    [Fact]
    public async Task UseSquidStd_WithoutAddSquidStdSerilog_KeepsDefaultLoggerFactory()
    {
        using var temp = new TempDirectory();
        var builder = CreateBuilder(temp.Path);
        builder.UseSquidStd(options => options.ConfigName = "app");

        await using var app = builder.Build();

        var factory = app.Services.GetRequiredService<ILoggerFactory>();
        Assert.IsNotType<SerilogLoggerFactory>(factory);
    }

    [Fact]
    public void AddSquidStdSerilog_WithoutUseSquidStd_Throws()
    {
        using var temp = new TempDirectory();
        var builder = CreateBuilder(temp.Path);

        var ex = Assert.Throws<InvalidOperationException>(() => builder.AddSquidStdSerilog());
        Assert.Equal("AddSquidStdSerilog must be called after UseSquidStd.", ex.Message);
    }

    [Fact]
    public async Task AddSquidStdSerilog_RoutesFrameworkLogsToSquidStdFileSink()
    {
        using var temp = new TempDirectory();
        var logDir = Path.Combine(temp.Path, "logs");
        File.WriteAllText(
            Path.Combine(temp.Path, "app.yaml"),
            "logger:\n  MinimumLevel: Information\n  EnableConsole: false\n  EnableFile: true\n  LogDirectory: logs\n  FileName: app-.log\n  RollingInterval: Day\n");

        const string marker = "framework-log-marker-a1b2c3";

        var builder = CreateBuilder(temp.Path);
        builder.UseSquidStd(options => options.ConfigName = "app");
        builder.AddSquidStdSerilog();

        await using (var app = builder.Build())
        {
            await app.StartAsync();
            var logger = app.Services.GetRequiredService<ILogger<SquidStdSerilogExtensionsTests>>();
            logger.LogInformation("emitting {Marker}", marker);
            await app.StopAsync();
        }

        await SerilogLog.CloseAndFlushAsync();

        var logFile = Directory.EnumerateFiles(logDir, "app-*.log").First();
        var contents = await File.ReadAllTextAsync(logFile);
        Assert.Contains(marker, contents, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddSquidStdSerilog_CalledTwice_DoesNotThrow()
    {
        using var temp = new TempDirectory();
        var builder = CreateBuilder(temp.Path);
        builder.UseSquidStd(options => options.ConfigName = "app");

        builder.AddSquidStdSerilog();
        builder.AddSquidStdSerilog();

        await using var app = builder.Build();
        var factory = app.Services.GetRequiredService<ILoggerFactory>();
        Assert.IsType<SerilogLoggerFactory>(factory);
    }

    private static WebApplicationBuilder CreateBuilder(string contentRootPath)
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ContentRootPath = contentRootPath,
                EnvironmentName = Environments.Development
            }
        );

        builder.WebHost.UseTestServer();

        return builder;
    }
}
