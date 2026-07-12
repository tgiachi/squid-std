// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

namespace SquidStd.Core.Collections.PriorityQueues;

/// <summary>
/// A <see cref="SimplePriorityQueue{TItem,TPriority}" /> fixed to a <see cref="float" /> priority, kept for
/// call sites that don't need a custom priority type. Prefer <see cref="SimplePriorityQueue{TItem,TPriority}" />
/// directly for new code.
/// Unlike System.Collections.Generic.PriorityQueue, supports UpdatePriority, O(1) Contains and efficient Remove.
/// </summary>
/// <typeparam name="TItem">The item type to enqueue.</typeparam>
public class SimplePriorityQueue<TItem> : SimplePriorityQueue<TItem, float>
{
    /// <summary>
    /// Instantiates a new priority queue using the default comparer.
    /// </summary>
    public SimplePriorityQueue()
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="comparer">The comparer used to compare priority values. Defaults to <see cref="Comparer{Single}.Default" />.</param>
    public SimplePriorityQueue(IComparer<float> comparer) : base(comparer)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="comparer">The comparison function used to compare priority values.</param>
    public SimplePriorityQueue(Comparison<float> comparer) : base(comparer)
    {
    }
}
