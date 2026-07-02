using System.Text;
using SquidStd.Core.Directories;
using SquidStd.Database.Abstractions.Data.Database;
using SquidStd.Database.Data;
using SquidStd.Database.Services;
using SquidStd.Vfs.Database.Data.Entities;
using SquidStd.Vfs.Database.Services;

namespace SquidStd.Tests.Vfs.Database;

public sealed class DatabaseFileSystemTests : IAsyncLifetime
{
    private string _dbPath = string.Empty;
    private DatabaseService _service = null!;
    private DatabaseFileSystem _fs = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), "squidstd-vfsdb-" + Guid.NewGuid().ToString("N") + ".db");
        _service = new(
            new DatabaseConfig
            {
                ConnectionString = $"sqlite://{_dbPath}",
                AutoMigrate = true
            },
            new DirectoriesConfig(Path.GetTempPath(), [])
        );
        await _service.StartAsync();
        _fs = new DatabaseFileSystem(NewAccess());
    }

    public async Task DisposeAsync()
    {
        await _service.StopAsync();

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task WriteRead_RoundTrip()
    {
        var data = Encoding.UTF8.GetBytes("hello database");
        await _fs.WriteAllBytesAsync("docs/hello.txt", data);

        var result = await _fs.ReadAllBytesAsync("docs/hello.txt");

        Assert.NotNull(result);
        Assert.Equal("hello database", Encoding.UTF8.GetString(result));
    }

    [Fact]
    public async Task WriteAsync_SamePath_Twice_ExactlyOneRow_LatestContent()
    {
        var access = NewAccess();
        var fs = new DatabaseFileSystem(access);

        await fs.WriteAllBytesAsync("file.txt", Encoding.UTF8.GetBytes("first"));
        await fs.WriteAllBytesAsync("file.txt", Encoding.UTF8.GetBytes("second"));

        // Verify only one row exists and it has the latest content.
        var rows = await access.QueryAsync(e => e.Path == "file.txt");
        Assert.Single(rows);
        Assert.Equal("second", Encoding.UTF8.GetString(rows[0].Content));
    }

    [Fact]
    public async Task ExistsAsync_TrueForWritten_FalseForMissing()
    {
        await _fs.WriteAllBytesAsync("present.bin", new byte[] { 0x01, 0x02 });

        Assert.True(await _fs.ExistsAsync("present.bin"));
        Assert.False(await _fs.ExistsAsync("absent.bin"));
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_ReturnsTrue_SubsequentReturnsFalse()
    {
        await _fs.WriteAllBytesAsync("todelete.txt", Encoding.UTF8.GetBytes("bye"));

        Assert.True(await _fs.DeleteAsync("todelete.txt"));
        Assert.False(await _fs.DeleteAsync("todelete.txt"));
        Assert.Null(await _fs.ReadAllBytesAsync("todelete.txt"));
    }

    [Fact]
    public async Task DeleteAsync_MissingFile_ReturnsFalse()
    {
        Assert.False(await _fs.DeleteAsync("nope.txt"));
    }

    [Fact]
    public async Task ListAsync_WithPrefix_ReturnsMatchingPaths()
    {
        await _fs.WriteAllBytesAsync("img/a.png", new byte[] { 1 });
        await _fs.WriteAllBytesAsync("img/b.png", new byte[] { 2 });
        await _fs.WriteAllBytesAsync("doc/c.txt", new byte[] { 3 });

        var paths = new List<string>();

        await foreach (var entry in _fs.ListAsync("img"))
        {
            paths.Add(entry.Path);
        }

        Assert.Contains("img/a.png", paths);
        Assert.Contains("img/b.png", paths);
        Assert.DoesNotContain("doc/c.txt", paths);
    }

    [Fact]
    public async Task ListAsync_NoPrefix_ReturnsAllPaths()
    {
        await _fs.WriteAllBytesAsync("x/1.dat", new byte[] { 10 });
        await _fs.WriteAllBytesAsync("y/2.dat", new byte[] { 20 });

        var paths = new List<string>();

        await foreach (var entry in _fs.ListAsync())
        {
            paths.Add(entry.Path);
        }

        Assert.Contains("x/1.dat", paths);
        Assert.Contains("y/2.dat", paths);
    }

    private FreeSqlDataAccess<VfsFileEntity> NewAccess()
        => new(_service);
}
