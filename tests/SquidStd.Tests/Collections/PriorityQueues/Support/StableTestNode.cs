using SquidStd.Core.Collections.PriorityQueues;

namespace SquidStd.Tests.Collections.PriorityQueues.Support;

internal sealed class StableTestNode : StablePriorityQueueNode
{
    public string Name { get; init; } = string.Empty;
}
