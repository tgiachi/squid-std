// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

namespace SquidStd.Core.Collections.PriorityQueues;

/// <summary>
/// The bookkeeping base class an item must extend to be enqueued in a <see cref="StablePriorityQueue{T}" />.
/// </summary>
public class StablePriorityQueueNode : FastPriorityQueueNode
{
    /// <summary>
    /// The order this node was inserted in, used to break ties between nodes of equal priority.
    /// </summary>
    public long InsertionIndex { get; internal set; }
}
