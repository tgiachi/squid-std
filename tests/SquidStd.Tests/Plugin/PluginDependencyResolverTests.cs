using SquidStd.Plugin.Exceptions;
using SquidStd.Plugin.Internal;
using SquidStd.Tests.Plugin.Support;

namespace SquidStd.Tests.Plugin;

public class PluginDependencyResolverTests
{
    private static string[] Ids(IEnumerable<SquidStd.Plugin.Abstractions.Interfaces.Plugins.ISquidStdPlugin> plugins)
        => plugins.Select(p => p.Metadata.Id).ToArray();

    [Fact]
    public void Sort_NoDependencies_KeepsRegistrationOrder()
    {
        var sorted = PluginDependencyResolver.Sort([new StubPlugin("c"), new StubPlugin("a"), new StubPlugin("b")]);

        Assert.Equal(["c", "a", "b"], Ids(sorted));
    }

    [Fact]
    public void Sort_DependencyComesFirst()
    {
        var sorted = PluginDependencyResolver.Sort([new StubPlugin("app", "core"), new StubPlugin("core")]);

        Assert.Equal(["core", "app"], Ids(sorted));
    }

    [Fact]
    public void Sort_TransitiveChain_IsOrdered()
    {
        var sorted = PluginDependencyResolver.Sort(
            [new StubPlugin("web", "app"), new StubPlugin("app", "core"), new StubPlugin("core")]
        );

        Assert.Equal(["core", "app", "web"], Ids(sorted));
    }

    [Fact]
    public void Sort_DependencyIdsAreCaseInsensitive()
    {
        var sorted = PluginDependencyResolver.Sort([new StubPlugin("App", "CORE"), new StubPlugin("core")]);

        Assert.Equal(["core", "App"], Ids(sorted));
    }

    [Fact]
    public void Sort_DuplicateId_Throws()
    {
        var ex = Assert.Throws<PluginLoadException>(
            () => PluginDependencyResolver.Sort([new StubPlugin("dup"), new StubPlugin("DUP")])
        );

        Assert.Contains("dup", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sort_MissingDependency_ThrowsNamingBoth()
    {
        var ex = Assert.Throws<PluginLoadException>(
            () => PluginDependencyResolver.Sort([new StubPlugin("app", "ghost")])
        );

        Assert.Contains("app", ex.Message);
        Assert.Contains("ghost", ex.Message);
    }

    [Fact]
    public void Sort_Cycle_Throws()
    {
        var ex = Assert.Throws<PluginLoadException>(
            () => PluginDependencyResolver.Sort([new StubPlugin("a", "b"), new StubPlugin("b", "a")])
        );

        Assert.Contains("cycle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sort_SelfDependency_Throws()
        => Assert.Throws<PluginLoadException>(() => PluginDependencyResolver.Sort([new StubPlugin("a", "a")]));
}
