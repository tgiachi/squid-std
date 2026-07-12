// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SquidStd.Core.Interfaces.Collections;

namespace SquidStd.Core.Collections.PriorityQueues;

/// <summary>
/// A simplified, thread-safe priority queue that stores plain items directly, without requiring them to extend a
/// node base class - at the cost of extra bookkeeping overhead compared to <see cref="FastPriorityQueue{T}" />.
/// Reach for this when items can't reasonably subclass a node type, or when Enqueue/Dequeue/UpdatePriority need
/// to be safe to call from multiple threads without external locking. Is stable and auto-resizes.
/// Unlike System.Collections.Generic.PriorityQueue, supports UpdatePriority, O(1) Contains and efficient Remove.
/// Methods documented as O(1) or O(log n) assume no duplicate items; duplicates may increase the algorithmic
/// complexity of the affected operations.
/// </summary>
/// <typeparam name="TItem">The item type to enqueue.</typeparam>
/// <typeparam name="TPriority">The priority type used to order items.</typeparam>
public class SimplePriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority>
{
    private sealed class SimpleNode : GenericPriorityQueueNode<TPriority>
    {
        public TItem Data { get; }

        public SimpleNode(TItem data)
        {
            Data = data;
        }
    }

    private const int InitialQueueSize = 10;
    private readonly GenericPriorityQueue<SimpleNode, TPriority> _queue;
    private readonly Dictionary<TItem, IList<SimpleNode>> _itemToNodesCache;
    private readonly IList<SimpleNode> _nullNodesCache;

    #region Constructors

    /// <summary>
    /// Instantiates a new priority queue using the default comparer and equality comparer.
    /// </summary>
    public SimplePriorityQueue() : this(Comparer<TPriority>.Default, EqualityComparer<TItem>.Default)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="priorityComparer">The comparer used to compare <typeparamref name="TPriority" /> values. Defaults to <see cref="Comparer{TPriority}.Default" />.</param>
    public SimplePriorityQueue(IComparer<TPriority> priorityComparer) : this(priorityComparer.Compare, EqualityComparer<TItem>.Default)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="priorityComparer">The comparison function used to compare <typeparamref name="TPriority" /> values.</param>
    public SimplePriorityQueue(Comparison<TPriority> priorityComparer) : this(priorityComparer, EqualityComparer<TItem>.Default)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="itemEquality">The equality comparer used to compare <typeparamref name="TItem" /> values.</param>
    public SimplePriorityQueue(IEqualityComparer<TItem> itemEquality) : this(Comparer<TPriority>.Default, itemEquality)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="priorityComparer">The comparer used to compare <typeparamref name="TPriority" /> values. Defaults to <see cref="Comparer{TPriority}.Default" />.</param>
    /// <param name="itemEquality">The equality comparer used to compare <typeparamref name="TItem" /> values.</param>
    public SimplePriorityQueue(IComparer<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) : this(priorityComparer.Compare, itemEquality)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="priorityComparer">The comparison function used to compare <typeparamref name="TPriority" /> values.</param>
    /// <param name="itemEquality">The equality comparer used to compare <typeparamref name="TItem" /> values.</param>
    public SimplePriorityQueue(Comparison<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality)
    {
        _queue = new GenericPriorityQueue<SimpleNode, TPriority>(InitialQueueSize, priorityComparer);
        _itemToNodesCache = new Dictionary<TItem, IList<SimpleNode>>(itemEquality);
        _nullNodesCache = new List<SimpleNode>();
    }

    #endregion

    /// <summary>
    /// Given an item, returns the earliest-enqueued <see cref="SimpleNode" /> still tracking it, or <c>null</c> if
    /// the item isn't enqueued.
    /// </summary>
    private SimpleNode? GetExistingNode(TItem item)
    {
        if (item == null)
        {
            return _nullNodesCache.Count > 0 ? _nullNodesCache[0] : null;
        }

        return !_itemToNodesCache.TryGetValue(item, out var nodes) ? null : nodes[0];
    }

    /// <summary>
    /// Adds a node to the item-cache, letting most operations run in O(1) or O(log n).
    /// </summary>
    private void AddToNodeCache(SimpleNode node)
    {
        if (node.Data == null)
        {
            _nullNodesCache.Add(node);
            return;
        }

        if (!_itemToNodesCache.TryGetValue(node.Data, out var nodes))
        {
            nodes = new List<SimpleNode>();
            _itemToNodesCache[node.Data] = nodes;
        }

        nodes.Add(node);
    }

    /// <summary>
    /// Removes a node from the item-cache, letting most operations run in O(1) or O(log n) (assuming no duplicates).
    /// </summary>
    private void RemoveFromNodeCache(SimpleNode node)
    {
        if (node.Data == null)
        {
            _nullNodesCache.Remove(node);
            return;
        }

        if (!_itemToNodesCache.TryGetValue(node.Data, out var nodes))
        {
            return;
        }

        nodes.Remove(node);
        if (nodes.Count == 0)
        {
            _itemToNodesCache.Remove(node.Data);
        }
    }

    /// <summary>
    /// Returns the number of items in the queue.
    /// O(1)
    /// </summary>
    public int Count
    {
        get
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }
    }

    /// <summary>
    /// Returns the head of the queue, without removing it (use <see cref="Dequeue" /> for that). Throws when the
    /// queue is empty.
    /// O(1)
    /// </summary>
    public TItem First
    {
        get
        {
            lock (_queue)
            {
                if (_queue.Count <= 0)
                {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }

                return _queue.First.Data;
            }
        }
    }

    /// <summary>
    /// Removes every item from the queue.
    /// O(n)
    /// </summary>
    public void Clear()
    {
        lock (_queue)
        {
            _queue.Clear();
            _itemToNodesCache.Clear();
            _nullNodesCache.Clear();
        }
    }

    /// <summary>
    /// Returns whether the given item is in the queue.
    /// O(1)
    /// </summary>
    /// <param name="item">The item to look for.</param>
    public bool Contains(TItem item)
    {
        lock (_queue)
        {
            return item == null ? _nullNodesCache.Count > 0 : _itemToNodesCache.ContainsKey(item);
        }
    }

    /// <summary>
    /// Removes the head of the queue (the item with minimum priority; ties are broken by order of insertion), and
    /// returns it. Throws when the queue is empty.
    /// O(log n)
    /// </summary>
    public TItem Dequeue()
    {
        lock (_queue)
        {
            if (_queue.Count <= 0)
            {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }

            var node = _queue.Dequeue();
            RemoveFromNodeCache(node);
            return node.Data;
        }
    }

    /// <summary>
    /// Enqueues the item with the given priority, without locking on <c>_queue</c> or updating the item-cache.
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    /// <param name="priority">The priority to enqueue the item at.</param>
    private SimpleNode EnqueueNoLockOrCache(TItem item, TPriority priority)
    {
        var node = new SimpleNode(item);
        if (_queue.Count == _queue.MaxSize)
        {
            _queue.Resize(_queue.MaxSize * 2 + 1);
        }

        _queue.Enqueue(node, priority);
        return node;
    }

    /// <summary>
    /// Enqueues an item to the priority queue. Lower values are placed in front. Ties are broken by
    /// first-in-first-out. This queue auto-resizes, so it never becomes 'full'. Duplicates and null values are
    /// allowed.
    /// O(log n)
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    /// <param name="priority">The priority to enqueue the item at.</param>
    public void Enqueue(TItem item, TPriority priority)
    {
        lock (_queue)
        {
            IList<SimpleNode> nodes;
            if (item == null)
            {
                nodes = _nullNodesCache;
            }
            else if (!_itemToNodesCache.TryGetValue(item, out nodes!))
            {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[item] = nodes;
            }

            var node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
        }
    }

    /// <summary>
    /// Enqueues an item to the priority queue only if it isn't already present. Lower values are placed in front.
    /// Ties are broken by first-in-first-out. This queue auto-resizes, so it never becomes 'full'. Null values
    /// are allowed.
    /// O(log n)
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    /// <param name="priority">The priority to enqueue the item at.</param>
    /// <returns><c>true</c> if the item was enqueued; <c>false</c> if it was already present.</returns>
    public bool EnqueueWithoutDuplicates(TItem item, TPriority priority)
    {
        lock (_queue)
        {
            IList<SimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count > 0)
                {
                    return false;
                }

                nodes = _nullNodesCache;
            }
            else if (_itemToNodesCache.ContainsKey(item))
            {
                return false;
            }
            else
            {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[item] = nodes;
            }

            var node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
            return true;
        }
    }

    /// <summary>
    /// Removes an item from the queue. The item does not need to be the head of the queue. If the item is not in
    /// the queue, throws. If multiple copies of the item are enqueued, only the earliest-added one is removed.
    /// O(log n)
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public void Remove(TItem item)
    {
        lock (_queue)
        {
            SimpleNode removeMe;
            IList<SimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count == 0)
                {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                }

                removeMe = _nullNodesCache[0];
                nodes = _nullNodesCache;
            }
            else
            {
                if (!_itemToNodesCache.TryGetValue(item, out nodes!))
                {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                }

                removeMe = nodes[0];
                if (nodes.Count == 1)
                {
                    _itemToNodesCache.Remove(item);
                }
            }

            _queue.Remove(removeMe);
            nodes.Remove(removeMe);
        }
    }

    /// <summary>
    /// Changes the priority of an item already in the queue. Throws if the item isn't enqueued. If the item is
    /// enqueued multiple times, only the earliest-added copy is updated. (If your requirements need every copy
    /// updated, wrap items in a wrapper type so they can be distinguished.)
    /// O(log n)
    /// </summary>
    /// <param name="item">The item whose priority should change.</param>
    /// <param name="priority">The new priority.</param>
    public void UpdatePriority(TItem item, TPriority priority)
    {
        lock (_queue)
        {
            var updateMe = GetExistingNode(item);
            if (updateMe == null)
            {
                throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + item);
            }

            _queue.UpdatePriority(updateMe, priority);
        }
    }

    /// <summary>
    /// Returns the priority of the given item. Throws if the item isn't enqueued. If the item is enqueued
    /// multiple times, only the priority of the earliest-added copy is returned.
    /// O(1)
    /// </summary>
    /// <param name="item">The item to look up.</param>
    public TPriority GetPriority(TItem item)
    {
        lock (_queue)
        {
            var findMe = GetExistingNode(item);
            if (findMe == null)
            {
                throw new InvalidOperationException("Cannot call GetPriority() on a node which is not enqueued: " + item);
            }

            return findMe.Priority;
        }
    }

    #region Try* methods for multithreading

    /// <summary>
    /// Gets the head of the queue, without removing it (use <see cref="TryDequeue" /> for that). Useful for
    /// multi-threading, where the queue may become empty between calls to <see cref="Contains" /> and <see cref="First" />.
    /// O(1)
    /// </summary>
    /// <param name="first">Set to the head of the queue if this returns <c>true</c>.</param>
    /// <returns><c>true</c> if the queue was non-empty; <c>false</c> otherwise.</returns>
    public bool TryFirst([MaybeNullWhen(false)] out TItem first)
    {
        if (_queue.Count > 0)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    first = _queue.First.Data;
                    return true;
                }
            }
        }

        first = default;
        return false;
    }

    /// <summary>
    /// Removes the head of the queue (the item with minimum priority; ties are broken by order of insertion), and
    /// sets it to <paramref name="first" />. Useful for multi-threading, where the queue may become empty between
    /// calls to <see cref="Contains" /> and <see cref="Dequeue" />.
    /// O(log n)
    /// </summary>
    /// <param name="first">Set to the removed item if this returns <c>true</c>.</param>
    /// <returns><c>true</c> if an item was removed; <c>false</c> if the queue was empty.</returns>
    public bool TryDequeue([MaybeNullWhen(false)] out TItem first)
    {
        if (_queue.Count > 0)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    var node = _queue.Dequeue();
                    first = node.Data;
                    RemoveFromNodeCache(node);
                    return true;
                }
            }
        }

        first = default;
        return false;
    }

    /// <summary>
    /// Attempts to remove an item from the queue. The item does not need to be the head of the queue. Useful for
    /// multi-threading, where the queue may become empty between calls to <see cref="Contains" /> and <see cref="Remove" />.
    /// If multiple copies of the item are enqueued, only the earliest-added one is removed.
    /// O(log n)
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was removed; <c>false</c> if it wasn't in the queue.</returns>
    public bool TryRemove(TItem item)
    {
        lock (_queue)
        {
            SimpleNode removeMe;
            IList<SimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count == 0)
                {
                    return false;
                }

                removeMe = _nullNodesCache[0];
                nodes = _nullNodesCache;
            }
            else
            {
                if (!_itemToNodesCache.TryGetValue(item, out nodes!))
                {
                    return false;
                }

                removeMe = nodes[0];
                if (nodes.Count == 1)
                {
                    _itemToNodesCache.Remove(item);
                }
            }

            _queue.Remove(removeMe);
            nodes.Remove(removeMe);
            return true;
        }
    }

    /// <summary>
    /// Changes the priority of an item already in the queue. Useful for multi-threading, where the queue may
    /// become empty between calls to <see cref="Contains" /> and <see cref="UpdatePriority" />. If the item is
    /// enqueued multiple times, only the earliest-added copy is updated. (If your requirements need every copy
    /// updated, wrap items in a wrapper type so they can be distinguished.)
    /// O(log n)
    /// </summary>
    /// <param name="item">The item whose priority should change.</param>
    /// <param name="priority">The new priority.</param>
    /// <returns><c>true</c> if the priority was updated; <c>false</c> otherwise.</returns>
    public bool TryUpdatePriority(TItem item, TPriority priority)
    {
        lock (_queue)
        {
            var updateMe = GetExistingNode(item);
            if (updateMe == null)
            {
                return false;
            }

            _queue.UpdatePriority(updateMe, priority);
            return true;
        }
    }

    /// <summary>
    /// Attempts to get the priority of the given item. Useful for multi-threading, where the queue may become
    /// empty between calls to <see cref="Contains" /> and <see cref="GetPriority" />. If the item is enqueued
    /// multiple times, only the priority of the earliest-added copy is returned.
    /// O(1)
    /// </summary>
    /// <param name="item">The item to look up.</param>
    /// <param name="priority">Set to the item's priority if this returns <c>true</c>.</param>
    /// <returns><c>true</c> if the item was found in the queue; <c>false</c> otherwise.</returns>
    public bool TryGetPriority(TItem item, [MaybeNullWhen(false)] out TPriority priority)
    {
        lock (_queue)
        {
            var findMe = GetExistingNode(item);
            if (findMe == null)
            {
                priority = default;
                return false;
            }

            priority = findMe.Priority;
            return true;
        }
    }

    #endregion

    /// <summary>
    /// Returns an enumerator over the items currently in the queue. The enumeration order reflects heap storage
    /// order, not priority order.
    /// </summary>
    public IEnumerator<TItem> GetEnumerator()
    {
        var queueData = new List<TItem>();
        lock (_queue)
        {
            //Copy to a separate list because we don't want to 'yield return' inside a lock
            foreach (var node in _queue)
            {
                queueData.Add(node.Data);
            }
        }

        return queueData.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// <b>Should not be called in production code.</b> Checks that the queue and its item-cache are still in a
    /// consistent state. Used for testing/debugging the queue.
    /// </summary>
    public bool IsValidQueue()
    {
        lock (_queue)
        {
            // Check all items in cache are in the queue
            foreach (var nodes in _itemToNodesCache.Values)
            {
                foreach (var node in nodes)
                {
                    if (!_queue.Contains(node))
                    {
                        return false;
                    }
                }
            }

            // Check all items in queue are in cache
            foreach (var node in _queue)
            {
                if (GetExistingNode(node.Data) == null)
                {
                    return false;
                }
            }

            // Check queue structure itself
            return _queue.IsValidQueue();
        }
    }
}
