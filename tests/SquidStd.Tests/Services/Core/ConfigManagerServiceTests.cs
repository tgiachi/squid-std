using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Config;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Services.Core.Services;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Services.Core;

public class ConfigManagerServiceTests
{
    [Fact]
    public void ConfigPath_AppendsYamlExtension()
    {
        using var container = new Container();
        var config = SquidStdConfig.Load("app", "/tmp/config");
        IConfigManagerService manager = new ConfigManagerService(config, container);

        Assert.Equal(Path.Combine("/tmp/config", "app.yaml"), manager.ConfigPath);
    }

    [Fact]
    public void GetConfig_ReturnsSectionBoundAtRegistration()
    {
        // Under the config-first contract, RegisterConfigSection binds eagerly as soon as a
        // SquidStdConfig instance is registered into the container - there is no separate
        // "start" step to wait for.
        using var temp = new TempDirectory();
        using var container = new Container();
        var config = SquidStdConfig.Load("app", temp.Path);
        container.RegisterInstance(config, IfAlreadyRegistered.Replace);
        container.RegisterConfigSection<TestConfig>("test");
        IConfigManagerService manager = new ConfigManagerService(config, container);

        Assert.Same(container.Resolve<TestConfig>(), manager.GetConfig<TestConfig>());
    }

    [Fact]
    public void Ctor_ExistingFile_BindsSectionEagerlyFromFile()
    {
        using var temp = new TempDirectory();
        File.WriteAllText(
            temp.Combine("app.yaml"),
            """
            test:
              Name: loaded
              Count: 9
            """
        );
        using var container = new Container();
        var config = SquidStdConfig.Load("app", temp.Path);
        container.RegisterInstance(config, IfAlreadyRegistered.Replace);
        container.RegisterConfigSection("test", static () => new TestConfig { Name = "default", Count = 3 });
        IConfigManagerService manager = new ConfigManagerService(config, container);

        var result = manager.GetConfig<TestConfig>();
        Assert.Equal("loaded", result.Name);
        Assert.Equal(9, result.Count);
    }

    [Fact]
    public void Save_MissingFile_CreatesDefaultFileWithRegisteredSection()
    {
        using var temp = new TempDirectory();
        using var container = new Container();
        var config = SquidStdConfig.Load("app", temp.Path);
        container.RegisterInstance(config, IfAlreadyRegistered.Replace);
        container.RegisterConfigSection("test", static () => new TestConfig { Name = "default", Count = 3 });
        IConfigManagerService manager = new ConfigManagerService(config, container);

        manager.Save();

        var path = Path.Combine(temp.Path, "app.yaml");
        var result = manager.GetConfig<TestConfig>();

        Assert.True(File.Exists(path));
        Assert.Equal("default", result.Name);
        Assert.Equal(3, result.Count);
        Assert.Contains("test:", File.ReadAllText(path));
    }

    [Fact]
    public void Save_MissingSection_UsesDefaultAndOmitsUntrackedSections()
    {
        using var temp = new TempDirectory();
        File.WriteAllText(
            temp.Combine("app.yaml"),
            """
            other:
              Enabled: true
            """
        );
        using var container = new Container();
        var config = SquidStdConfig.Load("app", temp.Path);
        container.RegisterInstance(config, IfAlreadyRegistered.Replace);
        container.RegisterConfigSection("test", static () => new TestConfig { Name = "default", Count = 3 });
        IConfigManagerService manager = new ConfigManagerService(config, container);

        manager.Save();

        var result = manager.GetConfig<TestConfig>();
        var yaml = File.ReadAllText(temp.Combine("app.yaml"));

        Assert.Equal("default", result.Name);
        Assert.Equal(3, result.Count);
        Assert.Contains("test:", yaml);
        Assert.DoesNotContain("other:", yaml);
    }
}
