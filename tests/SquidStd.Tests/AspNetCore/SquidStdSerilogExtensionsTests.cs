using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SquidStd.AspNetCore.Extensions;
using SquidStd.Tests.Support;

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
        Assert.Equal("SerilogLoggerFactory", factory.GetType().Name);
    }

    [Fact]
    public async Task UseSquidStd_WithoutAddSquidStdSerilog_KeepsDefaultLoggerFactory()
    {
        using var temp = new TempDirectory();
        var builder = CreateBuilder(temp.Path);
        builder.UseSquidStd(options => options.ConfigName = "app");

        await using var app = builder.Build();

        var factory = app.Services.GetRequiredService<ILoggerFactory>();
        Assert.NotEqual("SerilogLoggerFactory", factory.GetType().Name);
    }

    [Fact]
    public void AddSquidStdSerilog_WithoutUseSquidStd_Throws()
    {
        using var temp = new TempDirectory();
        var builder = CreateBuilder(temp.Path);

        var ex = Assert.Throws<InvalidOperationException>(() => builder.AddSquidStdSerilog());
        Assert.Equal("AddSquidStdSerilog must be called after UseSquidStd.", ex.Message);
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
