using SquidStd.Network.Client;
using SquidStd.Network.Interfaces.Middleware;

namespace SquidStd.Tests.Support;

/// <summary>
/// Test middleware that appends a marker byte to every payload, on both receive and send paths.
/// Used to verify pipeline ordering and transformation.
/// </summary>
public sealed class AppendingMiddleware : INetMiddleware
{
    private readonly byte _marker;

    public AppendingMiddleware(byte marker)
    {
        _marker = marker;
    }

    public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
        SquidStdTcpClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
        => ValueTask.FromResult<ReadOnlyMemory<byte>>(Append(data));

    public ValueTask<ReadOnlyMemory<byte>> ProcessSendAsync(
        SquidStdTcpClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
        => ValueTask.FromResult<ReadOnlyMemory<byte>>(Append(data));

    private byte[] Append(ReadOnlyMemory<byte> data)
    {
        var result = new byte[data.Length + 1];
        data.CopyTo(result);
        result[^1] = _marker;

        return result;
    }
}
