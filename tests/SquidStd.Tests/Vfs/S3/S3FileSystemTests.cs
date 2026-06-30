using System.Text;
using SquidStd.Tests.Storage.S3;
using SquidStd.Vfs.S3.Data;
using SquidStd.Vfs.S3.Services;

namespace SquidStd.Tests.Vfs.S3;

[Collection(MinioCollection.Name)]
public class S3FileSystemTests
{
    private readonly MinioContainerFixture _fixture;

    public S3FileSystemTests(MinioContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Ctor_MissingServiceUrl_Throws()
        => Assert.ThrowsAny<ArgumentException>(
            () => new S3FileSystem(new() { Aws = new() { AccessKey = "a", SecretKey = "b" }, Bucket = "c" })
        );

    [Fact]
    public async Task WriteAllBytesAsync_Then_ReadAllBytesAsync_RoundTrips()
    {
        using var fs = NewFileSystem();
        var path = Path();
        var data = Encoding.UTF8.GetBytes("hello s3 vfs");

        await fs.WriteAllBytesAsync(path, data);
        var read = await fs.ReadAllBytesAsync(path);

        Assert.NotNull(read);
        Assert.Equal(data, read);
    }

    [Fact]
    public async Task OpenWriteAsync_Then_OpenReadAsync_RoundTrips()
    {
        using var fs = NewFileSystem();
        var path = Path();
        var data = Encoding.UTF8.GetBytes("deferred write stream");

        await using (var write = await fs.OpenWriteAsync(path))
        {
            await write.WriteAsync(data);
        }

        await using var read = await fs.OpenReadAsync(path);
        using var buffer = new MemoryStream();
        await read.CopyToAsync(buffer);

        Assert.Equal(data, buffer.ToArray());
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueAfterWrite_AndFalseForMissing()
    {
        using var fs = NewFileSystem();
        var path = Path();

        Assert.False(await fs.ExistsAsync(path));

        await fs.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("x"));

        Assert.True(await fs.ExistsAsync(path));
    }

    [Fact]
    public async Task DeleteAsync_ExistingObject_ReturnsTrueAndRemoves()
    {
        using var fs = NewFileSystem();
        var path = Path();

        await fs.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("delete me"));

        Assert.True(await fs.DeleteAsync(path));
        Assert.False(await fs.ExistsAsync(path));
    }

    [Fact]
    public async Task DeleteAsync_MissingObject_ReturnsFalse()
    {
        using var fs = NewFileSystem();

        Assert.False(await fs.DeleteAsync(Path()));
    }

    [Fact]
    public async Task ReadAllBytesAsync_MissingObject_ReturnsNull()
    {
        using var fs = NewFileSystem();

        Assert.Null(await fs.ReadAllBytesAsync(Path()));
    }

    [Fact]
    public async Task OpenReadAsync_MissingObject_ThrowsFileNotFoundException()
    {
        using var fs = NewFileSystem();

        await Assert.ThrowsAsync<FileNotFoundException>(() => fs.OpenReadAsync(Path()));
    }

    [Fact]
    public async Task ListAsync_Prefix_ReturnsWrittenKeys()
    {
        using var fs = NewFileSystem();
        var prefix = "list-" + Guid.NewGuid().ToString("N") + "/";

        await fs.WriteAllBytesAsync(prefix + "a.txt", Encoding.UTF8.GetBytes("1"));
        await fs.WriteAllBytesAsync(prefix + "b.txt", Encoding.UTF8.GetBytes("2"));

        var entries = new List<string>();

        await foreach (var entry in fs.ListAsync(prefix))
        {
            entries.Add(entry.Path);
        }

        Assert.Equal(2, entries.Count);
        Assert.Contains(prefix + "a.txt", entries);
        Assert.Contains(prefix + "b.txt", entries);
    }

    private static string Path()
        => "vfs-" + Guid.NewGuid().ToString("N");

    private S3FileSystem NewFileSystem()
        => new(new S3FileSystemOptions
        {
            Aws = new()
            {
                ServiceUrl = _fixture.ServiceUrl,
                AccessKey = _fixture.AccessKey,
                SecretKey = _fixture.SecretKey
            },
            Bucket = "vfs-tests-" + Guid.NewGuid().ToString("N")[..8]
        });
}
