using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
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
}
