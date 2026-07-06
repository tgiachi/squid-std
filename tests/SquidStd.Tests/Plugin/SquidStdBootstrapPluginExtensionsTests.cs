using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Plugin.Abstractions.Data;
using SquidStd.Plugin.Abstractions.Interfaces.Plugins;
using SquidStd.Plugin.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Plugin.Support;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Plugin;

public class SquidStdBootstrapPluginExtensionsTests
{
    private sealed class RecorderPlugin : ISquidStdPlugin
    {
        private readonly List<string> _order;

        public PluginMetadata Metadata { get; }

        public IContainer? SeenContainer { get; private set; }

        public PluginContext? SeenContext { get; private set; }

        public RecorderPlugin(string id, List<string> order, params string[] dependencies)
        {
            _order = order;
            Metadata = new()
            {
                Id = id,
                Name = id,
                Version = new(1, 0, 0),
                Author = "Tests",
                Dependencies = dependencies
            };
        }

        public void Configure(IContainer container, PluginContext context)
        {
            SeenContainer = container;
            SeenContext = context;
            _order.Add(Metadata.Id);
        }
    }

    private sealed class ThrowingPlugin : ISquidStdPlugin
    {
        public PluginMetadata Metadata { get; } = new()
        {
            Id = "tests.throwing",
            Name = "Throwing",
            Version = new(1, 0, 0),
            Author = "Tests"
        };

        public void Configure(IContainer container, PluginContext context)
            => throw new InvalidOperationException("boom");
    }

    [Fact]
    public async Task UsePlugins_ConfiguresInDependencyOrder_WithBootstrapContainerAndContext()
    {
        using var root = new TempDirectory();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "plugintest", RootDirectory = root.Path, AppName = "PluginHost" }
        );

        var order = new List<string>();
        var app = new RecorderPlugin("tests.app", order, "tests.core");
        var core = new RecorderPlugin("tests.core", order);

        bootstrap.UsePlugins(plugins => plugins.Add(app).Add(core));

        Assert.Equal(["tests.core", "tests.app"], order);
        Assert.Same(bootstrap.Container, app.SeenContainer);
        Assert.Equal(root.Path, core.SeenContext!.GetData<string>(PluginContextKeys.RootDirectory));
        Assert.Equal("PluginHost", core.SeenContext!.GetData<string>(PluginContextKeys.AppName));
    }

    [Fact]
    public async Task UsePlugins_LoadsExternalPluginsFromRelativeDirectory()
    {
        using var root = new TempDirectory();
        var pluginsDir = Directory.CreateDirectory(root.Combine("plugins"));
        PluginAssemblyFactory.CompilePluginAssembly(pluginsDir.FullName, "tests.external");

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "plugintest", RootDirectory = root.Path }
        );

        bootstrap.UsePlugins(plugins => plugins.FromDirectory("plugins"));

        Assert.Equal("tests.external", bootstrap.Container.Resolve<string>(serviceKey: "plugin-marker"));
    }

    [Fact]
    public async Task UsePlugins_AfterStart_Throws()
    {
        using var root = new TempDirectory();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "plugintest", RootDirectory = root.Path }
        );

        await bootstrap.StartAsync();

        try
        {
            Assert.Throws<InvalidOperationException>(() => bootstrap.UsePlugins(_ => { }));
        }
        finally
        {
            await bootstrap.StopAsync();
        }
    }

    [Fact]
    public async Task UsePlugins_ConfigureException_Propagates()
    {
        using var root = new TempDirectory();

        await using var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "plugintest", RootDirectory = root.Path }
        );

        Assert.Throws<InvalidOperationException>(
            () => bootstrap.UsePlugins(plugins => plugins.Add(new ThrowingPlugin()))
        );
    }
}
