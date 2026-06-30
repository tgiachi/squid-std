using SquidStd.Vfs.Abstractions;

namespace SquidStd.Tests.Vfs;

public class DeferredWriteStreamTests
{
    [Fact]
    public async Task DisposeAsync_FlushesWrittenBytesOnce()
    {
        byte[]? flushed = null;
        var flushes = 0;

        var stream = new DeferredWriteStream((bytes, _) =>
        {
            flushed = bytes;
            flushes++;

            return ValueTask.CompletedTask;
        });

        await stream.WriteAsync(new byte[] { 1, 2, 3 });
        await stream.DisposeAsync();

        Assert.Equal(new byte[] { 1, 2, 3 }, flushed);
        Assert.Equal(1, flushes);
    }

    [Fact]
    public void Dispose_Synchronous_AlsoFlushes()
    {
        byte[]? flushed = null;

        using (var stream = new DeferredWriteStream((bytes, _) =>
        {
            flushed = bytes;

            return ValueTask.CompletedTask;
        }))
        {
            stream.Write(new byte[] { 9 }, 0, 1);
        }

        Assert.Equal(new byte[] { 9 }, flushed);
    }
}
