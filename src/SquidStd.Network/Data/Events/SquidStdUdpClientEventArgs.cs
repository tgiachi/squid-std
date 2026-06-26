using SquidStd.Network.Client;

namespace SquidStd.Network.Data.Events;

/// <summary>
///     Event payload containing a UDP client instance.
/// </summary>
public sealed class SquidStdUdpClientEventArgs : EventArgs
{
    public SquidStdUdpClientEventArgs(SquidStdUdpClient client)
    {
        Client = client;
    }

    /// <summary>
    ///     Started or closed UDP client.
    /// </summary>
    public SquidStdUdpClient Client { get; }
}
