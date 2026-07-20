using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Config;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Abstractions;

public class RegisterConfigFileExtensionTests
{
    public sealed class AnotherConfig
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public void RegisterConfigFile_BindsSectionFromExternalFile()
    {
        using var root = new TempDirectory();
        using var external = new TempDirectory();
        File.WriteAllText(Path.Combine(external.Path, "sample.yaml"), "sample:\n  Name: fromfile\n  Count: 7\n");

        var primary = SquidStdConfig.Load("app", root.Path);
        using var container = new Container();
        container.RegisterInstance(primary);

        container.RegisterConfigFile<TestConfig>("sample", external.Path);

        var config = container.Resolve<TestConfig>();
        Assert.Equal("fromfile", config.Name);
        Assert.Equal(7, config.Count);
    }

    [Fact]
    public void RegisterConfigFile_AbsentFile_UsesDefaults()
    {
        using var root = new TempDirectory();
        using var external = new TempDirectory();

        var primary = SquidStdConfig.Load("app", root.Path);
        using var container = new Container();
        container.RegisterInstance(primary);

        container.RegisterConfigFile("sample", external.Path, createDefault: static () => new TestConfig { Name = "def", Count = 3 });

        var config = container.Resolve<TestConfig>();
        Assert.Equal("def", config.Name);
        Assert.Equal(3, config.Count);
    }

    [Fact]
    public void RegisterConfigFile_SameFileTwoSections_ShareOneLoader()
    {
        using var root = new TempDirectory();
        using var external = new TempDirectory();

        var primary = SquidStdConfig.Load("app", root.Path);
        using var container = new Container();
        container.RegisterInstance(primary);

        container.RegisterConfigFile<TestConfig>("first", external.Path, configName: "shared");
        container.RegisterConfigFile<AnotherConfig>("second", external.Path, configName: "shared");

        var registry = container.Resolve<ExternalConfigRegistry>();
        Assert.Single(registry.All);
        Assert.True(container.IsRegistered<TestConfig>());
        Assert.True(container.IsRegistered<AnotherConfig>());
    }

    [Fact]
    public void RegisterConfigFile_WithoutSquidStdConfig_RegistersDefault()
    {
        using var external = new TempDirectory();
        using var container = new Container();

        container.RegisterConfigFile("sample", external.Path, createDefault: static () => new TestConfig { Name = "bare", Count = 1 });

        Assert.Equal("bare", container.Resolve<TestConfig>().Name);
    }
}
