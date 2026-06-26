using System.Net;
using SquidStd.Network.Client;
using SquidStd.Network.Data;
using SquidStd.Network.Server;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Network;

public class TransportCodecTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task Codec_RoundTripsClientToServer()
    {
        var received = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var server = new SquidTcpServer(
            new(IPAddress.Loopback, 0),
            connectionPipelineFactory: () => new ConnectionPipeline(new CountingXorCodec(7))
        );
        server.OnDataReceived += (_, e) => received.TrySetResult(e.Data.ToArray());
        await server.StartAsync(CancellationToken.None);

        await using var client = await SquidStdTcpClient.ConnectAsync(
            new(IPAddress.Loopback, server.Port),
            codec: new CountingXorCodec(7)
        );

        var payload = new byte[] { 1, 2, 3, 4, 5 };
        await client.SendAsync(payload, CancellationToken.None);

        Assert.Equal(payload, await received.Task.WaitAsync(Timeout));
    }

    [Fact]
    public async Task Codec_SwapsAtomicallyMidConnection()
    {
        var msg1 = new byte[] { 1, 1, 1 };
        var msg2 = new byte[] { 2, 2, 2 };
        var first = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var server = new SquidTcpServer(
            new(IPAddress.Loopback, 0),
            connectionPipelineFactory: () => new ConnectionPipeline(new CountingXorCodec(10))
        );
        server.OnDataReceived += (_, e) =>
        {
            if (first.Task.IsCompleted)
            {
                second.TrySetResult(e.Data.ToArray());
            }
            else
            {
                e.Client.SwapCodec(new CountingXorCodec(20));
                first.TrySetResult(e.Data.ToArray());
            }
        };
        await server.StartAsync(CancellationToken.None);

        await using var client = await SquidStdTcpClient.ConnectAsync(
            new(IPAddress.Loopback, server.Port),
            codec: new CountingXorCodec(10)
        );

        await client.SendAsync(msg1, CancellationToken.None);
        Assert.Equal(msg1, await first.Task.WaitAsync(Timeout));

        client.SwapCodec(new CountingXorCodec(20));
        await client.SendAsync(msg2, CancellationToken.None);
        Assert.Equal(msg2, await second.Task.WaitAsync(Timeout));
    }
}
