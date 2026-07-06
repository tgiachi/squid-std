using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Config;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Abstractions;

public class RegisterConfigSectionExtensionTests
{
    [Fact]
    public void RegisterConfigSection_WithoutSquidStdConfig_RegistersDefaultFromFactory()
    {
        using var container = new Container();

        container.RegisterConfigSection(
            "test",
            static () => new TestConfig { Name = "default", Count = 2 },
            4
        );

        var config = container.Resolve<TestConfig>();

        Assert.Equal("default", config.Name);
        Assert.Equal(2, config.Count);
    }

    [Fact]
    public void RegisterConfigSection_ReturnsSameContainerForChaining()
    {
        using var container = new Container();

        var result = container.RegisterConfigSection<TestConfig>("test");

        Assert.Same(container, result);
    }

    [Fact]
    public void RegisterConfigSection_SameTypeAndSection_IsIdempotent()
    {
        using var container = new Container();

        container.RegisterConfigSection<TestConfig>("test");
        var first = container.Resolve<TestConfig>();

        container.RegisterConfigSection<TestConfig>("test");
        var second = container.Resolve<TestConfig>();

        Assert.Same(first, second);
    }

    [Fact]
    public void RegisterConfigSection_DuplicateTypeDifferentSectionWithoutSquidStdConfig_IsSkipped()
    {
        using var container = new Container();

        container.RegisterConfigSection<TestConfig>("first");

        var exception = Record.Exception(() => container.RegisterConfigSection<TestConfig>("second"));

        Assert.Null(exception);
    }

    [Fact]
    public void RegisterConfigSection_DuplicateSectionDifferentTypeWithoutSquidStdConfig_RegistersBothDefaults()
    {
        using var container = new Container();

        container.RegisterConfigSection("dup", static () => new TestConfig());
        container.RegisterConfigSection("dup", static () => new AnotherConfig());

        Assert.True(container.IsRegistered<TestConfig>());
        Assert.True(container.IsRegistered<AnotherConfig>());
    }

    [Fact]
    public void RegisterConfigSection_SameTypeAndSectionWithSquidStdConfig_IsIdempotent()
    {
        using var root = new TempDirectory();
        var config = SquidStdConfig.Load("app", root.Path);
        using var container = new Container();
        container.RegisterInstance(config);

        container.RegisterConfigSection("sample", static () => new TestConfig());
        var first = container.Resolve<TestConfig>();
        container.RegisterConfigSection("sample", static () => new TestConfig());

        Assert.Same(first, container.Resolve<TestConfig>());
    }
}

public class AnotherConfig
{
    public string Value { get; set; } = string.Empty;
}
