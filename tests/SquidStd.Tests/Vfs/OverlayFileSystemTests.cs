using SquidStd.Vfs.Services;

namespace SquidStd.Tests.Vfs;

public class OverlayFileSystemTests
{
    [Fact]
    public async Task Read_PrefersOverlay_ThenBase()
    {
        var @base = new InMemoryFileSystem();
        var overlay = new InMemoryFileSystem();
        await @base.WriteAllBytesAsync("a.txt", new byte[] { 1 });
        await @base.WriteAllBytesAsync("b.txt", new byte[] { 2 });
        await overlay.WriteAllBytesAsync("a.txt", new byte[] { 99 });
        var fs = new OverlayFileSystem(@base, overlay);

        Assert.Equal(new byte[] { 99 }, await fs.ReadAllBytesAsync("a.txt")); // overlay shadows base
        Assert.Equal(new byte[] { 2 }, await fs.ReadAllBytesAsync("b.txt"));  // falls through to base
    }

    [Fact]
    public async Task Write_GoesToOverlay_BaseUntouched()
    {
        var @base = new InMemoryFileSystem();
        var overlay = new InMemoryFileSystem();
        var fs = new OverlayFileSystem(@base, overlay);

        await fs.WriteAllBytesAsync("c.txt", new byte[] { 7 });

        Assert.True(await overlay.ExistsAsync("c.txt"));
        Assert.False(await @base.ExistsAsync("c.txt"));
    }

    [Fact]
    public async Task Delete_BaseOnlyFile_ReturnsFalse()
    {
        var @base = new InMemoryFileSystem();
        var overlay = new InMemoryFileSystem();
        await @base.WriteAllBytesAsync("a.txt", new byte[] { 1 });
        var fs = new OverlayFileSystem(@base, overlay);

        Assert.False(await fs.DeleteAsync("a.txt")); // overlay has nothing to delete; base untouched
        Assert.True(await @base.ExistsAsync("a.txt"));
    }

    [Fact]
    public async Task List_IsUnion_OverlayShadowsBase()
    {
        var @base = new InMemoryFileSystem();
        var overlay = new InMemoryFileSystem();
        await @base.WriteAllBytesAsync("a.txt", new byte[] { 1 });
        await @base.WriteAllBytesAsync("b.txt", new byte[] { 2 });
        await overlay.WriteAllBytesAsync("a.txt", new byte[] { 99 });
        var fs = new OverlayFileSystem(@base, overlay);

        var paths = new List<string>();
        await foreach (var e in fs.ListAsync())
        {
            paths.Add(e.Path);
        }

        Assert.Equal(2, paths.Count); // a.txt once + b.txt
        Assert.Contains("a.txt", paths);
        Assert.Contains("b.txt", paths);
    }
}
