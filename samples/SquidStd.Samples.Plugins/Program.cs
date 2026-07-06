using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Plugin.Abstractions.Data;
using SquidStd.Plugin.Abstractions.Interfaces.Plugins;
using SquidStd.Plugin.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

namespace SquidStd.Samples.Plugins;

#region step-1

public interface IGreeter
{
    string Greet(string name);
}

public sealed class WeatherGreeter : IGreeter
{
    public string Greet(string name)
        => $"Hello {name}, the weather plugin is online.";
}

public sealed class WeatherPlugin : ISquidStdPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "squidstd.weather",
        Name = "Weather Plugin",
        Version = new(1, 0, 0),
        Author = "SquidStd Samples",
        Description = "Registers a greeter service.",
        Dependencies = []
    };

    public void Configure(IContainer container, PluginContext context)
        => container.Register<IGreeter, WeatherGreeter>(Reuse.Singleton);
}

#endregion

internal static class Program
{
    private static void Main()
    {
    #region step-2

        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "plugins-sample", RootDirectory = Directory.GetCurrentDirectory() }
        );

        var plugin = new WeatherPlugin();
        Console.WriteLine($"Loading {plugin.Metadata.Name} v{plugin.Metadata.Version} by {plugin.Metadata.Author}");

        bootstrap.UsePlugins(plugins => plugins.Add(plugin));

        var greeter = bootstrap.Resolve<IGreeter>();
        Console.WriteLine(greeter.Greet("squid"));

    #endregion
    }
}
