using System.Text;
using SquidStd.Vfs.Services;

namespace SquidStd.Tests.Vfs;

public class VfsDirectoriesTests
{
    [Fact]
    public async Task ResolvesNamedDirectoriesAndWritesUnderThem()
    {
        var fs = new InMemoryFileSystem();
        var dirs = new VfsDirectories(fs, ["data", "logs"]);

        Assert.Equal("data", dirs["data"]);
        Assert.Equal("logs", dirs["logs"]);

        var target = dirs.Combine("data", "cv.pdf");
        await fs.WriteAllBytesAsync(target, Encoding.UTF8.GetBytes("x"));

        Assert.Equal("data/cv.pdf", target);
        Assert.True(await fs.ExistsAsync("data/cv.pdf"));
    }
}
