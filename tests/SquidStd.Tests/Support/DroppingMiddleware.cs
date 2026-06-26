using SquidStd.Network.Client;
using SquidStd.Network.Interfaces.Middleware;

namespace SquidStd.Tests.Support;

/// <summary>
///     Test middleware that drops every payload by returning <see cref="ReadOnlyMemory{T}.Empty" />,
///     short-circuiting the pipeline.
/// </summary>
public sealed class DroppingMiddleware : INetMiddleware
{
    public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
        SquidStdTcpClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
    }

    public ValueTask<ReadOnlyMemory<byte>> ProcessSendAsync(
        SquidStdTcpClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
    }
}
