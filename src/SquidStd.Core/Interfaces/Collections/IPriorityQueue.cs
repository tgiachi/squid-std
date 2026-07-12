// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

namespace SquidStd.Core.Interfaces.Collections;

/// <summary>
/// The common priority-queue contract implemented by every queue in the
/// <see cref="SquidStd.Core.Collections.PriorityQueues" /> family. For raw speed, prefer accessing a queue through
/// its concrete type instead of this interface, since the JIT can optimize concrete-type calls better.
/// </summary>
/// <typeparam name="TItem">The values stored in the queue.</typeparam>
/// <typeparam name="TPriority">The priority type used to order items.</typeparam>
public interface IPriorityQueue<TItem, in TPriority> : IEnumerable<TItem>
{
    /// <summary>
    /// Enqueues an item to the priority queue. Lower priority values are placed in front. Ties are broken as
    /// documented by the implementing type.
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    /// <param name="priority">The priority to enqueue the item at.</param>
    void Enqueue(TItem item, TPriority priority);

    /// <summary>
    /// Removes the head of the queue (the item with minimum priority) and returns it.
    /// </summary>
    TItem Dequeue();

    /// <summary>
    /// Removes every item from the queue.
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns whether the given item is in the queue.
    /// </summary>
    /// <param name="item">The item to look for.</param>
    bool Contains(TItem item);

    /// <summary>
    /// Removes an item from the queue. The item does not need to be the head of the queue.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    void Remove(TItem item);

    /// <summary>
    /// Changes the priority of an item that is already in the queue.
    /// </summary>
    /// <param name="item">The item whose priority should change.</param>
    /// <param name="priority">The new priority.</param>
    void UpdatePriority(TItem item, TPriority priority);

    /// <summary>
    /// Returns the head of the queue, without removing it (use <see cref="Dequeue" /> for that).
    /// </summary>
    TItem First { get; }

    /// <summary>
    /// Returns the number of items in the queue.
    /// </summary>
    int Count { get; }
}
