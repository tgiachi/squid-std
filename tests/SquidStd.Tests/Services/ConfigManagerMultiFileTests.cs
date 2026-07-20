using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Config;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Services.Core.Services;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Services;

public class ConfigManagerMultiFileTests
{
    private static (IConfigManagerService Manager, IContainer Container, string ExternalDir) Build(TempDirectory root, TempDirectory external)
    {
        var primary = SquidStdConfig.Load("app", root.Path);
        var container = new Container();
        container.RegisterInstance(primary);
        container.RegisterConfigFile<TestConfig>("sample", external.Path);
        var manager = new ConfigManagerService(primary, container);
        container.RegisterInstance<IConfigManagerService>(manager);

        return (manager, container, external.Path);
    }

    [Fact]
    public void Save_WritesPrimaryAndExternalFiles()
    {
        using var root = new TempDirectory();
        using var external = new TempDirectory();
        var (manager, container, externalDir) = Build(root, external);

        container.Resolve<TestConfig>().Name = "persisted";
        manager.Save();

        Assert.True(File.Exists(Path.Combine(root.Path, "app.yaml")));
        var externalYaml = File.ReadAllText(Path.Combine(externalDir, "sample.yaml"));
        Assert.Contains("persisted", externalYaml);
    }

    [Fact]
    public void EnsureFiles_WritesMissingExternal_LeavesExistingPrimaryUntouched()
    {
        using var root = new TempDirectory();
        using var external = new TempDirectory();
        File.WriteAllText(Path.Combine(root.Path, "app.yaml"), "app:\n  Marker: keep\n");
        var (manager, _, externalDir) = Build(root, external);

        manager.EnsureFiles();

        Assert.Equal("app:\n  Marker: keep\n", File.ReadAllText(Path.Combine(root.Path, "app.yaml")));
        Assert.True(File.Exists(Path.Combine(externalDir, "sample.yaml")));
    }

    [Fact]
    public void Load_RebindsExternal_AndFiresConfigLoadedOnce()
    {
        using var root = new TempDirectory();
        using var external = new TempDirectory();
        var (manager, container, externalDir) = Build(root, external);

        File.WriteAllText(Path.Combine(externalDir, "sample.yaml"), "sample:\n  Name: reloaded\n  Count: 9\n");
        var fired = 0;
        manager.ConfigLoaded += () => fired++;

        manager.Load();

        Assert.Equal(1, fired);
        Assert.Equal("reloaded", container.Resolve<TestConfig>().Name);
    }
}
