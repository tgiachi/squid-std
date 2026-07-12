// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

namespace SquidStd.Core.Collections.PriorityQueues;

/// <summary>
/// The bookkeeping base class an item must extend to be enqueued in a
/// <see cref="GenericPriorityQueue{TItem,TPriority}" />.
/// </summary>
/// <typeparam name="TPriority">The priority type used to order nodes.</typeparam>
public class GenericPriorityQueueNode<TPriority>
{
    /// <summary>
    /// The priority this node was inserted at. Cannot be set directly - use
    /// <see cref="GenericPriorityQueue{TItem,TPriority}.Enqueue" /> or
    /// <see cref="GenericPriorityQueue{TItem,TPriority}.UpdatePriority" /> instead.
    /// </summary>
    public TPriority Priority { get; protected internal set; } = default!;

    /// <summary>
    /// The node's current position in the owning queue's backing array.
    /// </summary>
    public int QueueIndex { get; internal set; }

    /// <summary>
    /// The order this node was inserted in, used to break ties between nodes of equal priority.
    /// </summary>
    public long InsertionIndex { get; internal set; }

#if DEBUG
    /// <summary>
    /// The queue this node is currently tied to. Populated only in DEBUG builds, to validate that a node isn't
    /// used across multiple queues without first calling <see cref="GenericPriorityQueue{TItem,TPriority}.ResetNode" />.
    /// </summary>
    public object? Queue { get; internal set; }
#endif
}
