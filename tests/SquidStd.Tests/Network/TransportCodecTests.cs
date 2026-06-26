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
}
