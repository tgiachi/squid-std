using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.Services;

namespace SquidStd.Tests.Vfs;

public class ReadOnlyFileSystemTests
{
    private static async Task<IVirtualFileSystem> SeededInnerAsync()
    {
        var inner = new InMemoryFileSystem();
        await inner.WriteAllBytesAsync("a.txt", new byte[] { 1 });

        return inner;
    }

    [Fact]
    public async Task Reads_Delegate()
    {
        var fs = new ReadOnlyFileSystem(await SeededInnerAsync());

        Assert.True(await fs.ExistsAsync("a.txt"));
        Assert.Equal(new byte[] { 1 }, await fs.ReadAllBytesAsync("a.txt"));
    }

    [Fact]
    public async Task Writes_Throw()
    {
        var fs = new ReadOnlyFileSystem(await SeededInnerAsync());

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fs.WriteAllBytesAsync("b.txt", new byte[] { 2 }));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fs.DeleteAsync("a.txt"));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fs.OpenWriteAsync("b.txt"));
    }
}
