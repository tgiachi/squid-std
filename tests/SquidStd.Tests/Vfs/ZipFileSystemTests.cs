using System.Text;
using SquidStd.Vfs.Services;

namespace SquidStd.Tests.Vfs;

public class ZipFileSystemTests
{
    [Fact]
    public async Task Write_Reopen_Read_Delete_RoundTrips()
    {
        var path = Path.Combine(Path.GetTempPath(), "squidstd-vfs-" + Guid.NewGuid().ToString("N") + ".zip");

        try
        {
            await using (var fs = new ZipFileSystem(path))
            {
                await fs.WriteAllBytesAsync("docs/cv.pdf", Encoding.UTF8.GetBytes("hello"));
                await fs.WriteAllBytesAsync("note.txt", Encoding.UTF8.GetBytes("hi"));
            }

            await using (var fs = new ZipFileSystem(path))
            {
                Assert.Equal("hello", Encoding.UTF8.GetString((await fs.ReadAllBytesAsync("docs/cv.pdf"))!));
                Assert.True(await fs.DeleteAsync("note.txt"));
                Assert.False(await fs.ExistsAsync("note.txt"));
            }

            await using (var fs = new ZipFileSystem(path))
            {
                Assert.False(await fs.ExistsAsync("note.txt"));
                Assert.True(await fs.ExistsAsync("docs/cv.pdf"));
            }
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task ListAsync_AfterWritingInSameSession_DoesNotThrow_AndReportsSizes()
    {
        var path = Path.Combine(Path.GetTempPath(), "squidstd-vfs-" + Guid.NewGuid().ToString("N") + ".zip");

        try
        {
            await using var fs = new ZipFileSystem(path);
            await fs.WriteAllBytesAsync("a.txt", Encoding.UTF8.GetBytes("hello"));
            await fs.WriteAllBytesAsync("b.txt", Encoding.UTF8.GetBytes("hi"));

            var entries = new List<string>();
            long totalSize = 0;

            // In ZipArchiveMode.Update, ZipArchiveEntry.Length throws for entries written this
            // session; ListAsync must not surface that.
            await foreach (var entry in fs.ListAsync())
            {
                entries.Add(entry.Path);
                totalSize += entry.Size;
            }

            Assert.Contains("a.txt", entries);
            Assert.Contains("b.txt", entries);
            Assert.Equal(7, totalSize); // 5 + 2 uncompressed bytes
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
