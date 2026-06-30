using System.Net.Http;
using SquidStd.Vfs.Abstractions.Data;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.Services;

namespace SquidStd.Tests.Vfs;

public class CachingFileSystemTests
{
    // An InMemoryFileSystem-backed remote with a switch that makes every op throw a transport error.
    private sealed class FlakyRemote : IVirtualFileSystem
    {
        private readonly InMemoryFileSystem _inner = new();

        public bool Offline { get; set; }

        private void Guard()
        {
            if (Offline)
            {
                throw new HttpRequestException("offline");
            }
        }

        public ValueTask<bool> ExistsAsync(string path, CancellationToken ct = default) { Guard(); return _inner.ExistsAsync(path, ct); }
        public ValueTask<byte[]?> ReadAllBytesAsync(string path, CancellationToken ct = default) { Guard(); return _inner.ReadAllBytesAsync(path, ct); }
        public ValueTask WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken ct = default) { Guard(); return _inner.WriteAllBytesAsync(path, data, ct); }
        public Task<Stream> OpenReadAsync(string path, CancellationToken ct = default) { Guard(); return _inner.OpenReadAsync(path, ct); }
        public Task<Stream> OpenWriteAsync(string path, CancellationToken ct = default) { Guard(); return _inner.OpenWriteAsync(path, ct); }
        public ValueTask<bool> DeleteAsync(string path, CancellationToken ct = default) { Guard(); return _inner.DeleteAsync(path, ct); }
        public IAsyncEnumerable<VfsEntry> ListAsync(string? prefix = null, CancellationToken ct = default) { Guard(); return _inner.ListAsync(prefix, ct); }
    }

    [Fact]
    public async Task Read_PopulatesCache_AndServesFromCacheWhenOffline()
    {
        var remote = new FlakyRemote();
        var cache = new InMemoryFileSystem();
        var fs = new CachingFileSystem(remote, cache);

        await remote.WriteAllBytesAsync("a.txt", new byte[] { 1 });

        Assert.Equal(new byte[] { 1 }, await fs.ReadAllBytesAsync("a.txt")); // remote read populates cache
        Assert.True(await cache.ExistsAsync("a.txt"));

        remote.Offline = true;
        Assert.Equal(new byte[] { 1 }, await fs.ReadAllBytesAsync("a.txt")); // served from cache while offline
    }

    [Fact]
    public async Task Write_IsWriteThrough_AndFailsOffline()
    {
        var remote = new FlakyRemote();
        var cache = new InMemoryFileSystem();
        var fs = new CachingFileSystem(remote, cache);

        await fs.WriteAllBytesAsync("a.txt", new byte[] { 5 });
        Assert.True(await remote.ExistsAsync("a.txt"));
        Assert.True(await cache.ExistsAsync("a.txt"));

        remote.Offline = true;
        await Assert.ThrowsAsync<HttpRequestException>(async () => await fs.WriteAllBytesAsync("b.txt", new byte[] { 6 }));
    }

    [Fact]
    public async Task List_FallsBackToCache_WhenOffline()
    {
        var remote = new FlakyRemote();
        var cache = new InMemoryFileSystem();
        var fs = new CachingFileSystem(remote, cache);

        await remote.WriteAllBytesAsync("a.txt", new byte[] { 1 });
        await fs.ReadAllBytesAsync("a.txt"); // populate the cache while online

        remote.Offline = true; // remote.ListAsync now throws eagerly (HttpRequestException)

        var paths = new List<string>();
        await foreach (var e in fs.ListAsync())
        {
            paths.Add(e.Path);
        }

        Assert.Contains("a.txt", paths); // served from cache despite the remote being offline
    }

    [Fact]
    public async Task Read_MissingFile_DoesNotFallBackToCache()
    {
        var remote = new FlakyRemote();
        var cache = new InMemoryFileSystem();
        await cache.WriteAllBytesAsync("ghost.txt", new byte[] { 7 }); // stale cache entry
        var fs = new CachingFileSystem(remote, cache);

        // Remote is online and has no such file => null, NOT the stale cache copy.
        Assert.Null(await fs.ReadAllBytesAsync("ghost.txt"));
    }
}
