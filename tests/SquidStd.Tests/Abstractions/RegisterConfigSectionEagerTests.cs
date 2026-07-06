using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Config;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Abstractions;

public class RegisterConfigSectionEagerTests
{
    public sealed class EagerSection
    {
        public string Name { get; set; } = "default";
    }

    [Fact]
    public void RegisterConfigSection_WithSquidStdConfig_BindsImmediately()
    {
        using var root = new TempDirectory();
        File.WriteAllText(Path.Combine(root.Path, "app.yaml"), "eager:\n  Name: fromfile\n");
        var config = SquidStdConfig.Load("app", root.Path);
        using var container = new Container();
        container.RegisterInstance(config);

        container.RegisterConfigSection("eager", static () => new EagerSection());

        Assert.Equal("fromfile", container.Resolve<EagerSection>().Name);
        Assert.Contains(config.Entries, entry => entry.SectionName == "eager");
    }

    [Fact]
    public void RegisterConfigSection_ExplicitInstanceAlreadyRegistered_IsSkipped()
    {
        using var root = new TempDirectory();
        File.WriteAllText(Path.Combine(root.Path, "app.yaml"), "eager:\n  Name: fromfile\n");
        var config = SquidStdConfig.Load("app", root.Path);
        using var container = new Container();
        container.RegisterInstance(config);
        var explicitInstance = new EagerSection { Name = "explicit" };
        container.RegisterInstance(explicitInstance);

        container.RegisterConfigSection("eager", static () => new EagerSection());

        Assert.Same(explicitInstance, container.Resolve<EagerSection>());
        Assert.DoesNotContain(config.Entries, entry => entry.SectionName == "eager");
    }

    [Fact]
    public void RegisterConfigSection_TypeEagerlyTrackedUnderAnotherSection_Throws()
    {
        using var root = new TempDirectory();
        var config = SquidStdConfig.Load("app", root.Path);
        using var container = new Container();
        container.RegisterInstance(config);
        container.RegisterConfigSection("first", static () => new EagerSection());

        Assert.Throws<InvalidOperationException>(
            () => container.RegisterConfigSection<EagerSection>("second")
        );
    }

    [Fact]
    public void RegisterConfigSection_WithoutSquidStdConfig_RegistersDefaultInstance()
    {
        using var container = new Container();

        var exception = Record.Exception(
            () => container.RegisterConfigSection("legacy", static () => new EagerSection())
        );

        Assert.Null(exception);
        Assert.Equal("default", container.Resolve<EagerSection>().Name);
    }
}
