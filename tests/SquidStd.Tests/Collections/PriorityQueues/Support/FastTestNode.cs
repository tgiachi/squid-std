using SquidStd.Core.Collections.PriorityQueues;

namespace SquidStd.Tests.Collections.PriorityQueues.Support;

internal sealed class FastTestNode : FastPriorityQueueNode
{
    public string Name { get; init; } = string.Empty;
}
