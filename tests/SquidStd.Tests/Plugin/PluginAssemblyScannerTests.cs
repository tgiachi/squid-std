using SquidStd.Plugin.Exceptions;
using SquidStd.Plugin.Internal;
using SquidStd.Tests.Plugin.Support;

namespace SquidStd.Tests.Plugin;

public class PluginAssemblyScannerTests
{
    [Fact]
    public void Scan_MissingDirectory_Throws()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var ex = Assert.Throws<PluginLoadException>(() => PluginAssemblyScanner.Scan(missing));

        Assert.Contains(missing, ex.Message);
    }

    [Fact]
    public void Scan_DirectoryWithPluginAssembly_LoadsAndInstantiates()
    {
        var directory = Directory.CreateTempSubdirectory();

        try
        {
            PluginAssemblyFactory.CompilePluginAssembly(directory.FullName, "tests.generated");

            var plugins = PluginAssemblyScanner.Scan(directory.FullName);

            Assert.Single(plugins);
            Assert.Equal("tests.generated", plugins[0].Metadata.Id);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public void Scan_AssemblyWithoutPlugins_IsSkipped()
    {
        var directory = Directory.CreateTempSubdirectory();

        try
        {
            PluginAssemblyFactory.CompileNonPluginAssembly(directory.FullName);

            var plugins = PluginAssemblyScanner.Scan(directory.FullName);

            Assert.Empty(plugins);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public void Scan_CorruptDll_Throws()
    {
        var directory = Directory.CreateTempSubdirectory();

        try
        {
            File.WriteAllBytes(Path.Combine(directory.FullName, "broken.dll"), [1, 2, 3, 4]);

            Assert.Throws<PluginLoadException>(() => PluginAssemblyScanner.Scan(directory.FullName));
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
