using SquidStd.Core.Collections.PriorityQueues;
using SquidStd.Tests.Collections.PriorityQueues.Support;

namespace SquidStd.Tests.Collections.PriorityQueues;

public class StablePriorityQueueTests
{
    [Fact]
    public void Enqueue_MixedPriorities_DequeuesInAscendingPriorityOrder()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "a" };
        var b = new StableTestNode { Name = "b" };
        var c = new StableTestNode { Name = "c" };

        queue.Enqueue(c, 3f);
        queue.Enqueue(a, 1f);
        queue.Enqueue(b, 2f);

        Assert.Equal("a", queue.Dequeue().Name);
        Assert.Equal("b", queue.Dequeue().Name);
        Assert.Equal("c", queue.Dequeue().Name);
    }

    [Fact]
    public void Enqueue_EqualPriorities_DequeuesInFifoOrder()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "A" };
        var b = new StableTestNode { Name = "B" };
        var c = new StableTestNode { Name = "C" };

        queue.Enqueue(a, 5f);
        queue.Enqueue(b, 5f);
        queue.Enqueue(c, 1f);

        Assert.Equal("C", queue.Dequeue().Name);
        Assert.Equal("A", queue.Dequeue().Name);
        Assert.Equal("B", queue.Dequeue().Name);
    }

    [Fact]
    public void First_ReturnsHeadWithoutRemovingIt()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "a" };
        var b = new StableTestNode { Name = "b" };
        queue.Enqueue(b, 5f);
        queue.Enqueue(a, 1f);

        Assert.Equal("a", queue.First.Name);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void Count_TracksEnqueueAndDequeue()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        Assert.Equal(0, queue.Count);

        queue.Enqueue(new StableTestNode { Name = "a" }, 1f);
        queue.Enqueue(new StableTestNode { Name = "b" }, 2f);
        Assert.Equal(2, queue.Count);

        queue.Dequeue();
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void Enumeration_YieldsAllEnqueuedItems()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "a" };
        var b = new StableTestNode { Name = "b" };
        var c = new StableTestNode { Name = "c" };
        queue.Enqueue(a, 3f);
        queue.Enqueue(b, 1f);
        queue.Enqueue(c, 2f);

        var names = queue.Select(n => n.Name).ToHashSet();

        Assert.Equal(new HashSet<string> { "a", "b", "c" }, names);
    }

    [Fact]
    public void Resize_GrowsCapacity_AllowsEnqueueingBeyondOriginalMaxSize()
    {
        var queue = new StablePriorityQueue<StableTestNode>(2);
        queue.Enqueue(new StableTestNode { Name = "a" }, 1f);
        queue.Enqueue(new StableTestNode { Name = "b" }, 2f);
        Assert.Equal(2, queue.MaxSize);

        queue.Resize(4);
        queue.Enqueue(new StableTestNode { Name = "c" }, 0f);

        Assert.Equal(4, queue.MaxSize);
        Assert.Equal(3, queue.Count);
        Assert.Equal("c", queue.Dequeue().Name);
    }

    [Fact]
    public void UpdatePriority_LoweringValue_MovesNodeToFront()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "a" };
        var b = new StableTestNode { Name = "b" };
        var c = new StableTestNode { Name = "c" };
        queue.Enqueue(a, 1f);
        queue.Enqueue(b, 2f);
        queue.Enqueue(c, 3f);

        queue.UpdatePriority(c, 0f);

        Assert.Equal("c", queue.First.Name);
    }

    [Fact]
    public void UpdatePriority_RaisingValue_MovesNodeAwayFromFront()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "a" };
        var b = new StableTestNode { Name = "b" };
        queue.Enqueue(a, 1f);
        queue.Enqueue(b, 2f);

        queue.UpdatePriority(a, 5f);

        Assert.Equal("b", queue.First.Name);
    }

    [Fact]
    public void Remove_MiddleItem_RemovesWithoutBreakingOrder()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "a" };
        var b = new StableTestNode { Name = "b" };
        var c = new StableTestNode { Name = "c" };
        queue.Enqueue(a, 1f);
        queue.Enqueue(b, 2f);
        queue.Enqueue(c, 3f);

        queue.Remove(b);

        Assert.Equal(2, queue.Count);
        Assert.False(queue.Contains(b));
        Assert.Equal("a", queue.Dequeue().Name);
        Assert.Equal("c", queue.Dequeue().Name);
    }

    [Fact]
    public void Contains_TransitionsThroughEnqueueDequeueAndRemove()
    {
        var queue = new StablePriorityQueue<StableTestNode>(10);
        var a = new StableTestNode { Name = "a" };
        var b = new StableTestNode { Name = "b" };

        Assert.False(queue.Contains(a));

        queue.Enqueue(a, 1f);
        queue.Enqueue(b, 2f);
        Assert.True(queue.Contains(a));
        Assert.True(queue.Contains(b));

        queue.Remove(b);
        Assert.False(queue.Contains(b));

        queue.Dequeue();
        Assert.False(queue.Contains(a));
    }
}
