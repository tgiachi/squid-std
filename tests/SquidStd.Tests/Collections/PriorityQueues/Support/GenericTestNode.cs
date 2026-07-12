using SquidStd.Core.Collections.PriorityQueues;

namespace SquidStd.Tests.Collections.PriorityQueues.Support;

internal sealed class GenericTestNode<TPriority> : GenericPriorityQueueNode<TPriority>
{
    public string Name { get; init; } = string.Empty;
}
