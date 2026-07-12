// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

namespace SquidStd.Core.Interfaces.Collections;

/// <summary>
/// A helper interface implemented by the fixed-capacity queues (<see cref="SquidStd.Core.Collections.PriorityQueues.FastPriorityQueue{T}" />,
/// <see cref="SquidStd.Core.Collections.PriorityQueues.StablePriorityQueue{T}" /> and
/// <see cref="SquidStd.Core.Collections.PriorityQueues.GenericPriorityQueue{TItem,TPriority}" />) that back their storage with a
/// pre-sized array. Kept internal, mirroring upstream, since it exists mainly to make unit testing those queues easier.
/// </summary>
/// <typeparam name="TItem">The values stored in the queue.</typeparam>
/// <typeparam name="TPriority">The priority type used to order items.</typeparam>
internal interface IFixedSizePriorityQueue<TItem, in TPriority> : IPriorityQueue<TItem, TPriority>
{
    /// <summary>
    /// Resizes the queue so it can accept more items. All currently enqueued items are kept. Shrinking the queue
    /// below its current item count results in undefined behavior.
    /// </summary>
    /// <param name="maxNodes">The new maximum number of items the queue can hold.</param>
    void Resize(int maxNodes);

    /// <summary>
    /// Returns the maximum number of items that can be enqueued at once in this queue. Once <see cref="IPriorityQueue{TItem,TPriority}.Count" />
    /// reaches this value, enqueuing another item results in undefined behavior unless the queue is first resized.
    /// </summary>
    int MaxSize { get; }

    /// <summary>
    /// By default, an item that has been previously added to one queue cannot be added to another queue. Call
    /// this on the original queue before re-using the item elsewhere.
    /// </summary>
    /// <param name="node">The item to detach from this queue.</param>
    void ResetNode(TItem node);
}
