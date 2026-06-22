using System.Text;
using SquidStd.Storage.Services;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Storage;

public class FileStorageServiceTests
{
    [Fact]
    public async Task SaveAsync_LoadAsync_RoundTripsBytes()
    {
        using var temp = new TempDirectory();
        var service = new FileStorageService(new() { RootDirectory = temp.Path });
        var data = Encoding.UTF8.GetBytes("hello storage");

        await service.SaveAsync("profiles/main.bin", data);

        var loaded = await service.LoadAsync("profiles/main.bin");

        Assert.Equal(data, loaded);
        Assert.True(await service.ExistsAsync("profiles/main.bin"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesStoredValue()
    {
        using var temp = new TempDirectory();
        var service = new FileStorageService(new() { RootDirectory = temp.Path });
        await service.SaveAsync("cache/value.bin", new byte[] { 1, 2, 3 });

        var deleted = await service.DeleteAsync("cache/value.bin");

        Assert.True(deleted);
        Assert.False(await service.ExistsAsync("cache/value.bin"));
        Assert.Null(await service.LoadAsync("cache/value.bin"));
    }

    [Theory, InlineData("../escape.bin"), InlineData("/absolute.bin"), InlineData("nested/../../escape.bin")]
    public async Task SaveAsync_RejectsUnsafeKeys(string key)
    {
        using var temp = new TempDirectory();
        var service = new FileStorageService(new() { RootDirectory = temp.Path });

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(key, new byte[] { 1 }).AsTask());
    }

    [Fact]
    public async Task ObjectStorage_SaveAsync_LoadAsync_RoundTripsYamlObject()
    {
        using var temp = new TempDirectory();
        var storage = new FileStorageService(new() { RootDirectory = temp.Path });
        var objects = new YamlObjectStorageService(storage);
        var expected = new SampleObject
        {
            Name = "main",
            Value = 42
        };

        await objects.SaveAsync("objects/sample.yaml", expected);

        var actual = await objects.LoadAsync<SampleObject>("objects/sample.yaml");

        Assert.NotNull(actual);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Value, actual.Value);
    }

    private sealed class SampleObject
    {
        public string Name { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}
