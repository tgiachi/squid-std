using System.Net;
using SquidStd.Network.Sessions;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Network;

public class SessionTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_PopulatesProperties()
    {
        var connection = new FakeNetworkConnection(sessionId: 42, remoteEndPoint: new IPEndPoint(IPAddress.Loopback, 7000));
        var session = new Session<string>(42, connection, "state", CreatedAt);

        Assert.Equal(42, session.SessionId);
        Assert.Same(connection, session.Connection);
        Assert.Equal("state", session.State);
        Assert.Equal(CreatedAt, session.CreatedAtUtc);
        Assert.Equal(new IPEndPoint(IPAddress.Loopback, 7000), session.RemoteEndPoint);
        Assert.True(session.IsConnected);
    }

    [Fact]
    public async Task SendAsync_DelegatesToConnection()
    {
        var connection = new FakeNetworkConnection();
        var session = new Session<string>(1, connection, "s", CreatedAt);

        await session.SendAsync(new byte[] { 1, 2, 3 }, CancellationToken.None);

        Assert.Single(connection.SentPayloads);
        Assert.Equal([1, 2, 3], connection.SentPayloads[0]);
    }

    [Fact]
    public async Task CloseAsync_DelegatesToConnection()
    {
        var connection = new FakeNetworkConnection();
        var session = new Session<string>(1, connection, "s", CreatedAt);

        await session.CloseAsync();

        Assert.Equal(1, connection.CloseCount);
        Assert.False(session.IsConnected);
    }
}
