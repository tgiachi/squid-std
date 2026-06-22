using System.Net;
using SquidStd.Network.Client;
using SquidStd.Network.Server;
using SquidStd.Network.Types.Server;

namespace SquidStd.Tests.Network;

public class SquidStdUdpServerTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public void ServerType_IsUdp()
    {
        using var server = new SquidStdUdpServer(new IPEndPoint(IPAddress.Loopback, 0), bindAllInterfaces: false);

        Assert.Equal(ServerType.UDP, server.ServerType);
    }

    [Fact]
    public async Task StartStop_TogglesIsRunning()
    {
        await using var server = new SquidStdUdpServer(new IPEndPoint(IPAddress.Loopback, 0), bindAllInterfaces: false);

        Assert.False(server.IsRunning);
        await server.StartAsync(CancellationToken.None);
        Assert.True(server.IsRunning);

        await server.StopAsync(CancellationToken.None);
        Assert.False(server.IsRunning);

        // Start/Stop/Start cycle is supported.
        await server.StartAsync(CancellationToken.None);
        Assert.True(server.IsRunning);
    }

    [Fact]
    public async Task DefaultBehaviour_EchoesDatagramBackToSender()
    {
        await using var server = new SquidStdUdpServer(new IPEndPoint(IPAddress.Loopback, 0), bindAllInterfaces: false);
        await server.StartAsync(CancellationToken.None);
        var serverPort = server.Port;

        await using var client = new SquidStdUdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var received = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.OnDataReceived += (_, e) => received.TrySetResult(e.Data.ToArray());
        await client.StartAsync(CancellationToken.None);

        var payload = new byte[] { 1, 2, 3, 4, 5 };
        await client.SendToAsync(payload, new IPEndPoint(IPAddress.Loopback, serverPort), CancellationToken.None);

        Assert.Equal(payload, await received.Task.WaitAsync(Timeout));
    }

    [Fact]
    public async Task OnDatagram_CustomResponse_IsReturnedToSender()
    {
        await using var server = new SquidStdUdpServer(new IPEndPoint(IPAddress.Loopback, 0), bindAllInterfaces: false)
        {
            OnDatagram = (_, _) => new byte[] { 0xFF, 0xFE }
        };
        await server.StartAsync(CancellationToken.None);
        var serverPort = server.Port;

        await using var client = new SquidStdUdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var received = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.OnDataReceived += (_, e) => received.TrySetResult(e.Data.ToArray());
        await client.StartAsync(CancellationToken.None);

        await client.SendToAsync(new byte[] { 1 }, new IPEndPoint(IPAddress.Loopback, serverPort), CancellationToken.None);

        Assert.Equal([0xFF, 0xFE], await received.Task.WaitAsync(Timeout));
    }

    [Fact]
    public async Task BindSingleInterface_HasOneListener()
    {
        await using var server = new SquidStdUdpServer(new IPEndPoint(IPAddress.Loopback, 0), bindAllInterfaces: false);
        await server.StartAsync(CancellationToken.None);

        Assert.Equal(1, server.ListenerCount);
    }
}
