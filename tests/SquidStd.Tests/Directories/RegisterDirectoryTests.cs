using SquidStd.Core.Directories;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Directories;

public class RegisterDirectoryTests
{
    private enum TestDirType
    {
        SavedGames
    }

    [Fact]
    public void RegisterDirectory_CreatesAndReturnsPath()
    {
        using var root = new TempDirectory();
        var config = new DirectoriesConfig(root.Path, []);

        var path = config.RegisterDirectory("scripts");

        Assert.Equal(Path.Combine(root.Path, "scripts"), path);
        Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void RegisterDirectory_IsIdempotent()
    {
        using var root = new TempDirectory();
        var config = new DirectoriesConfig(root.Path, []);

        var first = config.RegisterDirectory("scripts");
        var second = config.RegisterDirectory("scripts");

        Assert.Equal(first, second);
    }

    [Fact]
    public void RegisterDirectory_UsesSnakeCase()
    {
        using var root = new TempDirectory();
        var config = new DirectoriesConfig(root.Path, []);

        var path = config.RegisterDirectory("SavedGames");

        Assert.Equal(Path.Combine(root.Path, "saved_games"), path);
        Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void RegisterDirectory_EnumOverload_MatchesGetPath()
    {
        using var root = new TempDirectory();
        var config = new DirectoriesConfig(root.Path, []);

        var path = config.RegisterDirectory(TestDirType.SavedGames);

        Assert.Equal(config.GetPath(TestDirType.SavedGames), path);
        Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void RegisterDirectory_BlankName_Throws()
    {
        using var root = new TempDirectory();
        var config = new DirectoriesConfig(root.Path, []);

        Assert.Throws<ArgumentException>(() => config.RegisterDirectory(" "));
    }
}
