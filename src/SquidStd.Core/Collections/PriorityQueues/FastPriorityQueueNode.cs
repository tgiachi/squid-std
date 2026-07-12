// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

namespace SquidStd.Core.Collections.PriorityQueues;

/// <summary>
/// The bookkeeping base class an item must extend to be enqueued in a <see cref="FastPriorityQueue{T}" />.
/// </summary>
public class FastPriorityQueueNode
{
    /// <summary>
    /// The priority this node was inserted at. Cannot be set directly - use <see cref="FastPriorityQueue{T}.Enqueue" />
    /// or <see cref="FastPriorityQueue{T}.UpdatePriority" /> instead.
    /// </summary>
    public float Priority { get; protected internal set; }

    /// <summary>
    /// The node's current position in the owning queue's backing array.
    /// </summary>
    public int QueueIndex { get; internal set; }

#if DEBUG
    /// <summary>
    /// The queue this node is currently tied to. Populated only in DEBUG builds, to validate that a node isn't
    /// used across multiple queues without first calling <see cref="FastPriorityQueue{T}.ResetNode" />.
    /// </summary>
    public object? Queue { get; internal set; }
#endif
}
