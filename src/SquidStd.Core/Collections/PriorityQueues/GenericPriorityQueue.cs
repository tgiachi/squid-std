// Adapted from High-Speed Priority Queue for C# by BlueRaja
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - MIT License

using System.Collections;
using System.Runtime.CompilerServices;
using SquidStd.Core.Interfaces.Collections;

namespace SquidStd.Core.Collections.PriorityQueues;

/// <summary>
/// A copy of <see cref="StablePriorityQueue{T}" /> that allows any <typeparamref name="TPriority" /> ordered by
/// <see cref="IComparer{T}" /> or <see cref="Comparison{T}" /> instead of being fixed to <see cref="float" />.
/// Reach for this when priorities are integers, timestamps, or need a custom comparer (e.g. a max-heap).
/// Unlike System.Collections.Generic.PriorityQueue, supports UpdatePriority, O(1) Contains and efficient Remove.
/// </summary>
/// <typeparam name="TItem">The item type stored in the queue. Must extend <see cref="GenericPriorityQueueNode{TPriority}" />.</typeparam>
/// <typeparam name="TPriority">The priority type used to order nodes.</typeparam>
public sealed class GenericPriorityQueue<TItem, TPriority> : IFixedSizePriorityQueue<TItem, TPriority>
    where TItem : GenericPriorityQueueNode<TPriority>
{
    private readonly Comparison<TPriority> _comparer;
    private int _numNodes;
    private TItem?[] _nodes;
    private long _numNodesEverEnqueued;

    /// <summary>
    /// Instantiates a new priority queue using the default comparer for <typeparamref name="TPriority" />.
    /// </summary>
    /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this results in undefined behavior).</param>
    public GenericPriorityQueue(int maxNodes) : this(maxNodes, Comparer<TPriority>.Default)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this results in undefined behavior).</param>
    /// <param name="comparer">The comparer used to compare <typeparamref name="TPriority" /> values.</param>
    public GenericPriorityQueue(int maxNodes, IComparer<TPriority> comparer) : this(maxNodes, comparer.Compare)
    {
    }

    /// <summary>
    /// Instantiates a new priority queue.
    /// </summary>
    /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this results in undefined behavior).</param>
    /// <param name="comparer">The comparison function used to compare <typeparamref name="TPriority" /> values.</param>
    public GenericPriorityQueue(int maxNodes, Comparison<TPriority> comparer)
    {
#if DEBUG
        if (maxNodes <= 0)
        {
            throw new InvalidOperationException("New queue size cannot be smaller than 1");
        }
#endif

        _numNodes = 0;
        _nodes = new TItem[maxNodes + 1];
        _numNodesEverEnqueued = 0;
        _comparer = comparer;
    }

    /// <summary>
    /// Returns the number of nodes in the queue.
    /// O(1)
    /// </summary>
    public int Count => _numNodes;

    /// <summary>
    /// Returns the maximum number of items that can be enqueued at once in this queue. Once <see cref="Count" />
    /// reaches this value, enqueuing another item results in undefined behavior unless <see cref="Resize" /> is
    /// called first.
    /// O(1)
    /// </summary>
    public int MaxSize => _nodes.Length - 1;

    /// <summary>
    /// Removes every node from the queue.
    /// O(n) (So, don't do this often!)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_nodes, 1, _numNodes);
        _numNodes = 0;
    }

    /// <summary>
    /// Returns (in O(1)!) whether the given node is in the queue. If the node is, or has been previously added
    /// to another queue, the result is undefined unless <c>oldQueue.ResetNode(node)</c> has been called.
    /// O(1)
    /// </summary>
    /// <param name="node">The node to look for.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(TItem node)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(node);

        if (node.Queue != null && !Equals(node.Queue))
        {
            throw new InvalidOperationException(
                "node.Contains was called on a node from another queue.  Please call originalQueue.ResetNode() first"
            );
        }

        if (node.QueueIndex < 0 || node.QueueIndex >= _nodes.Length)
        {
            throw new InvalidOperationException("node.QueueIndex has been corrupted. Did you change it manually?");
        }
#endif

        return _nodes[node.QueueIndex] == node;
    }

    /// <summary>
    /// Enqueues a node to the priority queue. Lower values are placed in front. Ties are broken by
    /// first-in-first-out. If the queue is full, the node is already enqueued, or the node belongs to another
    /// queue (without a preceding <see cref="ResetNode" />), the result is undefined.
    /// O(log n)
    /// </summary>
    /// <param name="node">The node to enqueue.</param>
    /// <param name="priority">The priority to enqueue the node at.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(TItem node, TPriority priority)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(node);

        if (_numNodes >= _nodes.Length - 1)
        {
            throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
        }

        if (node.Queue != null && !Equals(node.Queue))
        {
            throw new InvalidOperationException(
                "node.Enqueue was called on a node from another queue.  Please call originalQueue.ResetNode() first"
            );
        }

        if (Contains(node))
        {
            throw new InvalidOperationException("Node is already enqueued: " + node);
        }

        node.Queue = this;
#endif

        node.Priority = priority;
        _numNodes++;
        _nodes[_numNodes] = node;
        node.QueueIndex = _numNodes;
        node.InsertionIndex = _numNodesEverEnqueued++;
        CascadeUp(node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CascadeUp(TItem node)
    {
        //aka Heapify-up
        int parent;
        if (node.QueueIndex > 1)
        {
            parent = node.QueueIndex >> 1;
            var parentNode = _nodes[parent]!;
            if (HasHigherPriority(parentNode, node))
            {
                return;
            }

            //Node has lower priority value, so move parent down the heap to make room
            _nodes[node.QueueIndex] = parentNode;
            parentNode.QueueIndex = node.QueueIndex;

            node.QueueIndex = parent;
        }
        else
        {
            return;
        }

        while (parent > 1)
        {
            parent >>= 1;
            var parentNode = _nodes[parent]!;
            if (HasHigherPriority(parentNode, node))
            {
                break;
            }

            //Node has lower priority value, so move parent down the heap to make room
            _nodes[node.QueueIndex] = parentNode;
            parentNode.QueueIndex = node.QueueIndex;

            node.QueueIndex = parent;
        }

        _nodes[node.QueueIndex] = node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CascadeDown(TItem node)
    {
        //aka Heapify-down
        var finalQueueIndex = node.QueueIndex;
        var childLeftIndex = 2 * finalQueueIndex;

        // If leaf node, we're done
        if (childLeftIndex > _numNodes)
        {
            return;
        }

        // Check if the left-child is higher-priority than the current node
        var childRightIndex = childLeftIndex + 1;
        var childLeft = _nodes[childLeftIndex]!;
        if (HasHigherPriority(childLeft, node))
        {
            // Check if there is a right child. If not, swap and finish.
            if (childRightIndex > _numNodes)
            {
                node.QueueIndex = childLeftIndex;
                childLeft.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childLeft;
                _nodes[childLeftIndex] = node;
                return;
            }

            // Check if the left-child is higher-priority than the right-child
            var childRight = _nodes[childRightIndex]!;
            if (HasHigherPriority(childLeft, childRight))
            {
                // left is highest, move it up and continue
                childLeft.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childLeft;
                finalQueueIndex = childLeftIndex;
            }
            else
            {
                // right is even higher, move it up and continue
                childRight.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childRight;
                finalQueueIndex = childRightIndex;
            }
        }
        // Not swapping with left-child, does right-child exist?
        else if (childRightIndex > _numNodes)
        {
            return;
        }
        else
        {
            // Check if the right-child is higher-priority than the current node
            var childRight = _nodes[childRightIndex]!;
            if (HasHigherPriority(childRight, node))
            {
                childRight.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childRight;
                finalQueueIndex = childRightIndex;
            }
            // Neither child is higher-priority than current, so finish and stop.
            else
            {
                return;
            }
        }

        while (true)
        {
            childLeftIndex = 2 * finalQueueIndex;

            // If leaf node, we're done
            if (childLeftIndex > _numNodes)
            {
                node.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = node;
                break;
            }

            // Check if the left-child is higher-priority than the current node
            childRightIndex = childLeftIndex + 1;
            childLeft = _nodes[childLeftIndex]!;
            if (HasHigherPriority(childLeft, node))
            {
                // Check if there is a right child. If not, swap and finish.
                if (childRightIndex > _numNodes)
                {
                    node.QueueIndex = childLeftIndex;
                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    _nodes[childLeftIndex] = node;
                    break;
                }

                // Check if the left-child is higher-priority than the right-child
                var childRight = _nodes[childRightIndex]!;
                if (HasHigherPriority(childLeft, childRight))
                {
                    // left is highest, move it up and continue
                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                }
                else
                {
                    // right is even higher, move it up and continue
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }
            // Not swapping with left-child, does right-child exist?
            else if (childRightIndex > _numNodes)
            {
                node.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = node;
                break;
            }
            else
            {
                // Check if the right-child is higher-priority than the current node
                var childRight = _nodes[childRightIndex]!;
                if (HasHigherPriority(childRight, node))
                {
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                // Neither child is higher-priority than current, so finish and stop.
                else
                {
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Returns true if 'higher' has higher priority than 'lower' - via the configured comparer, falling back to
    /// insertion order when priorities compare equal - false otherwise.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasHigherPriority(TItem higher, TItem lower)
    {
        var cmp = _comparer(higher.Priority, lower.Priority);
        return cmp < 0 || (cmp == 0 && higher.InsertionIndex < lower.InsertionIndex);
    }

    /// <summary>
    /// Removes the head of the queue (the node with minimum priority; ties are broken by order of insertion), and
    /// returns it. If the queue is empty, the result is undefined.
    /// O(log n)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TItem Dequeue()
    {
#if DEBUG
        if (_numNodes <= 0)
        {
            throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
        }

        if (!IsValidQueue())
        {
            throw new InvalidOperationException(
                "Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()?" +
                "Or add the same node to two different queues?)"
            );
        }
#endif

        var returnMe = _nodes[1]!;
        //If the node is already the last node, we can remove it immediately
        if (_numNodes == 1)
        {
            _nodes[1] = null;
            _numNodes = 0;
            return returnMe;
        }

        //Swap the node with the last node
        var formerLastNode = _nodes[_numNodes]!;
        _nodes[1] = formerLastNode;
        formerLastNode.QueueIndex = 1;
        _nodes[_numNodes] = null;
        _numNodes--;

        //Now bubble formerLastNode (which is no longer the last node) down
        CascadeDown(formerLastNode);
        return returnMe;
    }

    /// <summary>
    /// Resizes the queue so it can accept more nodes. All currently enqueued nodes are kept. Attempting to
    /// decrease the queue size below the current node count results in undefined behavior.
    /// O(n)
    /// </summary>
    /// <param name="maxNodes">The new maximum number of nodes the queue can hold.</param>
    public void Resize(int maxNodes)
    {
#if DEBUG
        if (maxNodes <= 0)
        {
            throw new InvalidOperationException("Queue size cannot be smaller than 1");
        }

        if (maxNodes < _numNodes)
        {
            throw new InvalidOperationException(
                "Called Resize(" + maxNodes + "), but current queue contains " + _numNodes + " nodes"
            );
        }
#endif

        var newArray = new TItem[maxNodes + 1];
        var highestIndexToCopy = Math.Min(maxNodes, _numNodes);
        Array.Copy(_nodes, newArray, highestIndexToCopy + 1);
        _nodes = newArray;
    }

    /// <summary>
    /// Returns the head of the queue, without removing it (use <see cref="Dequeue" /> for that). If the queue is
    /// empty, the result is undefined.
    /// O(1)
    /// </summary>
    public TItem First
    {
        get
        {
#if DEBUG
            if (_numNodes <= 0)
            {
                throw new InvalidOperationException("Cannot call .First on an empty queue");
            }
#endif

            return _nodes[1]!;
        }
    }

    /// <summary>
    /// Must be called on a node every time its priority changes while it is in the queue.
    /// <b>Forgetting to call this method results in a corrupted queue!</b> Calling this on a node that isn't
    /// enqueued results in undefined behavior.
    /// O(log n)
    /// </summary>
    /// <param name="node">The node whose priority should change.</param>
    /// <param name="priority">The new priority.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdatePriority(TItem node, TPriority priority)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(node);

        if (node.Queue != null && !Equals(node.Queue))
        {
            throw new InvalidOperationException("node.UpdatePriority was called on a node from another queue");
        }

        if (!Contains(node))
        {
            throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + node);
        }
#endif

        node.Priority = priority;
        OnNodeUpdated(node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnNodeUpdated(TItem node)
    {
        //Bubble the updated node up or down as appropriate
        var parentIndex = node.QueueIndex >> 1;

        if (parentIndex > 0 && HasHigherPriority(node, _nodes[parentIndex]!))
        {
            CascadeUp(node);
        }
        else
        {
            //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
            CascadeDown(node);
        }
    }

    /// <summary>
    /// Removes a node from the queue. The node does not need to be the head of the queue. If the node is not in
    /// the queue, the result is undefined - check <see cref="Contains" /> first if unsure.
    /// O(log n)
    /// </summary>
    /// <param name="node">The node to remove.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(TItem node)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(node);

        if (node.Queue != null && !Equals(node.Queue))
        {
            throw new InvalidOperationException("node.Remove was called on a node from another queue");
        }

        if (!Contains(node))
        {
            throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + node);
        }
#endif

        //If the node is already the last node, we can remove it immediately
        if (node.QueueIndex == _numNodes)
        {
            _nodes[_numNodes] = null;
            _numNodes--;
            return;
        }

        //Swap the node with the last node
        var formerLastNode = _nodes[_numNodes]!;
        _nodes[node.QueueIndex] = formerLastNode;
        formerLastNode.QueueIndex = node.QueueIndex;
        _nodes[_numNodes] = null;
        _numNodes--;

        //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
        OnNodeUpdated(formerLastNode);
    }

    /// <summary>
    /// By default, a node that has been previously added to one queue cannot be added to another queue. Call
    /// this on the original queue before re-using the node elsewhere.
    /// </summary>
    /// <param name="node">The node to detach from this queue.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetNode(TItem node)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(node);

        if (node.Queue != null && !Equals(node.Queue))
        {
            throw new InvalidOperationException("node.ResetNode was called on a node from another queue");
        }

        if (Contains(node))
        {
            throw new InvalidOperationException("node.ResetNode was called on a node that is still in the queue");
        }

        node.Queue = null;
#endif

        node.QueueIndex = 0;
    }

    /// <summary>
    /// Returns an enumerator over the nodes currently in the queue. The enumeration order reflects heap storage
    /// order, not priority order.
    /// </summary>
    public IEnumerator<TItem> GetEnumerator()
    {
        IEnumerable<TItem> e = new ArraySegment<TItem>(_nodes!, 1, _numNodes);
        return e.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// <b>Should not be called in production code.</b> Checks that the queue is still in a valid state. Used for
    /// testing/debugging the queue.
    /// </summary>
    public bool IsValidQueue()
    {
        for (var i = 1; i < _nodes.Length; i++)
        {
            if (_nodes[i] != null)
            {
                var childLeftIndex = 2 * i;
                if (childLeftIndex < _nodes.Length && _nodes[childLeftIndex] != null &&
                    HasHigherPriority(_nodes[childLeftIndex]!, _nodes[i]!))
                {
                    return false;
                }

                var childRightIndex = childLeftIndex + 1;
                if (childRightIndex < _nodes.Length && _nodes[childRightIndex] != null &&
                    HasHigherPriority(_nodes[childRightIndex]!, _nodes[i]!))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
