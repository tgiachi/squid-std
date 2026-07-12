using SquidStd.Core.Collections.PriorityQueues;

namespace SquidStd.Tests.Collections.PriorityQueues;

public class SimplePriorityQueueTests
{
    [Fact]
    public void Enqueue_MixedPriorities_DequeuesInAscendingPriorityOrder()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("c", 3);
        queue.Enqueue("a", 1);
        queue.Enqueue("b", 2);

        Assert.Equal("a", queue.Dequeue());
        Assert.Equal("b", queue.Dequeue());
        Assert.Equal("c", queue.Dequeue());
    }

    [Fact]
    public void First_ReturnsHeadWithoutRemovingIt()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("b", 2);
        queue.Enqueue("a", 1);

        Assert.Equal("a", queue.First);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void Count_TracksEnqueueAndDequeue()
    {
        var queue = new SimplePriorityQueue<string, int>();
        Assert.Equal(0, queue.Count);

        queue.Enqueue("a", 1);
        queue.Enqueue("b", 2);
        Assert.Equal(2, queue.Count);

        queue.Dequeue();
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void Enumeration_YieldsAllEnqueuedItems()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("a", 3);
        queue.Enqueue("b", 1);
        queue.Enqueue("c", 2);

        var items = queue.ToHashSet();

        Assert.Equal(new HashSet<string> { "a", "b", "c" }, items);
    }

    [Fact]
    public void TryFirst_OnEmptyQueue_ReturnsFalse()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Assert.False(queue.TryFirst(out var first));
        Assert.Null(first);
    }

    [Fact]
    public void TryFirst_OnNonEmptyQueue_ReturnsTrueAndHead()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("b", 2);
        queue.Enqueue("a", 1);

        Assert.True(queue.TryFirst(out var first));
        Assert.Equal("a", first);
    }

    [Fact]
    public void TryDequeue_OnEmptyQueue_ReturnsFalse()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Assert.False(queue.TryDequeue(out var item));
        Assert.Null(item);
    }

    [Fact]
    public void TryDequeue_OnNonEmptyQueue_RemovesAndReturnsHead()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("b", 2);
        queue.Enqueue("a", 1);

        Assert.True(queue.TryDequeue(out var item));
        Assert.Equal("a", item);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void TryRemove_ItemNotInQueue_ReturnsFalse()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Assert.False(queue.TryRemove("missing"));
    }

    [Fact]
    public void TryRemove_ItemInQueue_RemovesAndReturnsTrue()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("a", 1);

        Assert.True(queue.TryRemove("a"));
        Assert.False(queue.Contains("a"));
    }

    [Fact]
    public void Enqueue_DuplicateItem_TracksBothCopiesAndOperatesOnEarliestAddedOne()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("x", 5);
        queue.Enqueue("x", 10);

        Assert.Equal(2, queue.Count);
        Assert.True(queue.Contains("x"));
        Assert.Equal(5, queue.GetPriority("x"));

        queue.Remove("x");

        Assert.Equal(1, queue.Count);
        Assert.True(queue.Contains("x"));
        Assert.Equal(10, queue.GetPriority("x"));
    }

    [Fact]
    public void Enqueue_FromMultipleThreads_AllItemsAreCounted()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Parallel.For(0, 1000, i => queue.Enqueue($"item{i}", i));

        Assert.Equal(1000, queue.Count);
    }

    [Fact]
    public void EnqueueWithoutDuplicates_SameItemTwice_EnqueuesOnlyOnce()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Assert.True(queue.EnqueueWithoutDuplicates("x", 1));
        Assert.False(queue.EnqueueWithoutDuplicates("x", 2));

        Assert.Equal(1, queue.Count);
        Assert.Equal(1, queue.GetPriority("x"));
    }

    [Fact]
    public void TryUpdatePriority_ItemNotInQueue_ReturnsFalse()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Assert.False(queue.TryUpdatePriority("missing", 1));
    }

    [Fact]
    public void TryUpdatePriority_ItemInQueue_ReturnsTrueAndReordersQueue()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("a", 1);
        queue.Enqueue("b", 2);

        Assert.True(queue.TryUpdatePriority("b", 0));

        Assert.Equal("b", queue.First);
    }

    [Fact]
    public void TryGetPriority_ItemNotInQueue_ReturnsFalse()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Assert.False(queue.TryGetPriority("missing", out var priority));
        Assert.Equal(0, priority);
    }

    [Fact]
    public void TryGetPriority_ItemInQueue_ReturnsTrueAndPriority()
    {
        var queue = new SimplePriorityQueue<string, int>();
        queue.Enqueue("a", 7);

        Assert.True(queue.TryGetPriority("a", out var priority));
        Assert.Equal(7, priority);
    }

    [Fact]
    public void Enqueue_NullItem_SupportsContainsAndDequeue()
    {
        var queue = new SimplePriorityQueue<string, int>();

        queue.Enqueue(null!, 1);

        Assert.True(queue.Contains(null!));
        Assert.Null(queue.Dequeue());
    }

    [Fact]
    public void UpdatePriority_ItemNotInQueue_ThrowsInvalidOperationException()
    {
        var queue = new SimplePriorityQueue<string, int>();

        Assert.Throws<InvalidOperationException>(() => queue.UpdatePriority("missing", 1));
    }
}
