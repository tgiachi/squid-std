using System.Text;
using SquidStd.Storage.S3.Services;

namespace SquidStd.Tests.Storage.S3;

[Collection(MinioCollection.Name)]
public class S3StorageServiceTests
{
    private readonly MinioContainerFixture _fixture;

    public S3StorageServiceTests(MinioContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Ctor_MissingServiceUrl_Throws()
        => Assert.ThrowsAny<ArgumentException>(
            () =>
                new S3StorageService(new() { Aws = new() { AccessKey = "a", SecretKey = "b" }, Bucket = "c" })
        );

    [Fact]
    public async Task Exists_And_Delete()
    {
        var storage = NewService();
        var key = Key();
        await storage.SaveAsync(key, Encoding.UTF8.GetBytes("v"));

        Assert.True(await storage.ExistsAsync(key));
        Assert.True(await storage.DeleteAsync(key));
        Assert.False(await storage.ExistsAsync(key));
        Assert.False(await storage.DeleteAsync(key));
    }

    [Fact]
    public async Task ListKeysAsync_ReturnsObjectsUnderPrefix()
    {
        var storage = NewService();
        var prefix = "list-" + Guid.NewGuid().ToString("N") + "/";
        await storage.SaveAsync(prefix + "a", Encoding.UTF8.GetBytes("1"));
        await storage.SaveAsync(prefix + "b", Encoding.UTF8.GetBytes("2"));

        var keys = new List<string>();

        await foreach (var key in storage.ListKeysAsync(prefix))
        {
            keys.Add(key);
        }

        Assert.Equal(2, keys.Count);
        Assert.Contains(prefix + "a", keys);
        Assert.Contains(prefix + "b", keys);
    }

    [Fact]
    public async Task Load_MissingKey_ReturnsNull()
        => Assert.Null(await NewService().LoadAsync(Key()));

    [Fact]
    public async Task SaveThenLoad_RoundTrips_AndCreatesBucket()
    {
        var storage = NewService();
        var key = Key();

        await storage.SaveAsync(key, Encoding.UTF8.GetBytes("hello"));
        var loaded = await storage.LoadAsync(key);

        Assert.NotNull(loaded);
        Assert.Equal("hello", Encoding.UTF8.GetString(loaded!));
    }

    private static string Key()
        => "k-" + Guid.NewGuid().ToString("N");

    private S3StorageService NewService()
        => new(
            new()
            {
                Aws = new()
                {
                    ServiceUrl = _fixture.ServiceUrl,
                    AccessKey = _fixture.AccessKey,
                    SecretKey = _fixture.SecretKey
                },
                Bucket = "squidstd-tests"
            }
        );
}
