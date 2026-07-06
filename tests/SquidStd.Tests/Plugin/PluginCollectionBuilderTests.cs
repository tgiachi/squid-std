using DryIoc;
using SquidStd.Plugin;
using SquidStd.Plugin.Abstractions.Data;
using SquidStd.Plugin.Abstractions.Interfaces.Plugins;

namespace SquidStd.Tests.Plugin;

public class PluginCollectionBuilderTests
{
    private sealed class AlphaPlugin : ISquidStdPlugin
    {
        public PluginMetadata Metadata { get; } = new()
        {
            Id = "tests.alpha",
            Name = "Alpha",
            Version = new(1, 0, 0),
            Author = "Tests"
        };

        public void Configure(IContainer container, PluginContext context) { }
    }

    [Fact]
    public void Add_ByTypeAndInstance_CollectsInOrder()
    {
        var builder = new PluginCollectionBuilder();
        var instance = new AlphaPlugin();

        builder.Add<AlphaPlugin>().Add(instance);

        Assert.Equal(2, builder.Plugins.Count);
        Assert.Same(instance, builder.Plugins[1]);
    }

    [Fact]
    public void Add_NullInstance_Throws()
        => Assert.Throws<ArgumentNullException>(() => new PluginCollectionBuilder().Add(null!));

    [Fact]
    public void FromDirectory_CollectsPaths()
    {
        var builder = new PluginCollectionBuilder();

        builder.FromDirectory("plugins").FromDirectory("extra");

        Assert.Equal(["plugins", "extra"], builder.Directories);
    }

    [Fact]
    public void FromDirectory_BlankPath_Throws()
        => Assert.Throws<ArgumentException>(() => new PluginCollectionBuilder().FromDirectory(" "));
}
