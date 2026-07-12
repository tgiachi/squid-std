using SquidStd.Core.Collections.PriorityQueues;
using SquidStd.Tests.Collections.PriorityQueues.Support;

namespace SquidStd.Tests.Collections.PriorityQueues;

public class GenericPriorityQueueTests
{
    [Fact]
    public void Enqueue_MixedPriorities_DequeuesInAscendingPriorityOrder()
    {
        var queue = new GenericPriorityQueue<GenericTestNode<int>, int>(10);
        var a = new GenericTestNode<int> { Name = "a" };
        var b = new GenericTestNode<int> { Name = "b" };
        var c = new GenericTestNode<int> { Name = "c" };

        queue.Enqueue(c, 3);
        queue.Enqueue(a, 1);
        queue.Enqueue(b, 2);

        Assert.Equal("a", queue.Dequeue().Name);
        Assert.Equal("b", queue.Dequeue().Name);
        Assert.Equal("c", queue.Dequeue().Name);
    }

    [Fact]
    public void First_ReturnsHeadWithoutRemovingIt()
    {
        var queue = new GenericPriorityQueue<GenericTestNode<int>, int>(10);
        var a = new GenericTestNode<int> { Name = "a" };
        var b = new GenericTestNode<int> { Name = "b" };
        queue.Enqueue(b, 5);
        queue.Enqueue(a, 1);

        Assert.Equal("a", queue.First.Name);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void Count_TracksEnqueueAndDequeue()
    {
        var queue = new GenericPriorityQueue<GenericTestNode<int>, int>(10);
        Assert.Equal(0, queue.Count);

        queue.Enqueue(new GenericTestNode<int> { Name = "a" }, 1);
        queue.Enqueue(new GenericTestNode<int> { Name = "b" }, 2);
        Assert.Equal(2, queue.Count);

        queue.Dequeue();
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void Enumeration_YieldsAllEnqueuedItems()
    {
        var queue = new GenericPriorityQueue<GenericTestNode<int>, int>(10);
        var a = new GenericTestNode<int> { Name = "a" };
        var b = new GenericTestNode<int> { Name = "b" };
        var c = new GenericTestNode<int> { Name = "c" };
        queue.Enqueue(a, 3);
        queue.Enqueue(b, 1);
        queue.Enqueue(c, 2);

        var names = queue.Select(n => n.Name).ToHashSet();

        Assert.Equal(new HashSet<string> { "a", "b", "c" }, names);
    }

    [Fact]
    public void CustomComparer_Descending_ProducesMaxQueueOrdering()
    {
        Comparison<int> descending = (x, y) => y.CompareTo(x);
        var queue = new GenericPriorityQueue<GenericTestNode<int>, int>(10, descending);
        var a = new GenericTestNode<int> { Name = "a" };
        var b = new GenericTestNode<int> { Name = "b" };
        var c = new GenericTestNode<int> { Name = "c" };

        queue.Enqueue(a, 1);
        queue.Enqueue(b, 3);
        queue.Enqueue(c, 2);

        Assert.Equal("b", queue.Dequeue().Name);
        Assert.Equal("c", queue.Dequeue().Name);
        Assert.Equal("a", queue.Dequeue().Name);
    }

    [Fact]
    public void LongPriorityType_DequeuesInAscendingOrder()
    {
        var queue = new GenericPriorityQueue<GenericTestNode<long>, long>(10);
        var a = new GenericTestNode<long> { Name = "a" };
        var b = new GenericTestNode<long> { Name = "b" };

        queue.Enqueue(a, 100_000_000_000L);
        queue.Enqueue(b, 1L);

        Assert.Equal("b", queue.Dequeue().Name);
        Assert.Equal("a", queue.Dequeue().Name);
    }
}
