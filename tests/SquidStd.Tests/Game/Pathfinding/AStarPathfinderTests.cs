using SquidStd.Game.Pathfinding;

namespace SquidStd.Tests.Game.Pathfinding;

public class AStarPathfinderTests
{
    private static AStarPathfinder<(int X, int Y)> CreateGridPathfinder(
        int width,
        int height,
        HashSet<(int X, int Y)>? blocked = null,
        Func<(int X, int Y), (int X, int Y), double>? cost = null
    )
    {
        blocked ??= [];

        return new(
            neighbors: p =>
            {
                var candidates = new (int X, int Y)[]
                {
                    (p.X + 1, p.Y), (p.X - 1, p.Y), (p.X, p.Y + 1), (p.X, p.Y - 1)
                };

                return candidates.Where(c => c.X >= 0 && c.X < width && c.Y >= 0 && c.Y < height && !blocked.Contains(c));
            },
            cost: cost ?? ((_, _) => 1.0),
            heuristic: (p, goal) => Math.Abs(p.X - goal.X) + Math.Abs(p.Y - goal.Y)
        );
    }

    private static double PathCost(
        IReadOnlyList<(int X, int Y)> path,
        Func<(int X, int Y), (int X, int Y), double> cost
    )
    {
        var total = 0.0;

        for (var i = 1; i < path.Count; i++)
        {
            total += cost(path[i - 1], path[i]);
        }

        return total;
    }

    [Fact]
    public void FindPath_StraightLine_IncludesStartAndGoal()
    {
        var pathfinder = CreateGridPathfinder(10, 10);

        var path = pathfinder.FindPath((0, 0), (4, 0));

        Assert.Equal(5, path.Count);
        Assert.Equal((0, 0), path[0]);
        Assert.Equal((4, 0), path[^1]);
    }

    [Fact]
    public void FindPath_DetoursAroundWall_AndStaysOptimal()
    {
        // vertical wall at x=2, gap at y=4
        var blocked = new HashSet<(int X, int Y)>(
            Enumerable.Range(0, 4).Select(y => (2, y))
        );
        var pathfinder = CreateGridPathfinder(6, 6, blocked);

        var path = pathfinder.FindPath((0, 0), (4, 0));

        Assert.Equal((0, 0), path[0]);
        Assert.Equal((4, 0), path[^1]);
        Assert.DoesNotContain(path, p => blocked.Contains(p));
        // optimal detour: down to y=4 around the wall and back = 12 steps -> 13 nodes
        Assert.Equal(13, path.Count);
    }

    [Fact]
    public void FindPath_Unreachable_ReturnsEmpty_AndTryReturnsFalse()
    {
        // goal fully walled in
        var blocked = new HashSet<(int X, int Y)> { (4, 5), (6, 5), (5, 4), (5, 6) };
        var pathfinder = CreateGridPathfinder(10, 10, blocked);

        var path = pathfinder.FindPath((0, 0), (5, 5));
        var found = pathfinder.TryFindPath((0, 0), (5, 5), out var tryPath);

        Assert.Empty(path);
        Assert.False(found);
        Assert.Empty(tryPath);
    }

    [Fact]
    public void FindPath_StartEqualsGoal_ReturnsSingleNode()
    {
        var pathfinder = CreateGridPathfinder(3, 3);

        var path = pathfinder.FindPath((1, 1), (1, 1));

        Assert.Single(path);
        Assert.Equal((1, 1), path[0]);
    }

    [Fact]
    public void FindPath_WeightedEdges_PrefersCheaperLongerRoute()
    {
        // moving through row y=0 costs 10, everything else costs 1:
        // best route from (0,0) to (4,0) dips to y=1
        double Cost((int X, int Y) from, (int X, int Y) to) => to.Y == 0 && to != (4, 0) ? 10.0 : 1.0;
        var pathfinder = CreateGridPathfinder(6, 6, cost: Cost);

        var path = pathfinder.FindPath((0, 0), (4, 0));

        Assert.Contains(path, p => p.Y == 1);
        // optimum dips to y=1 immediately, runs along it and re-enters at (4,0): six unit-cost edges
        Assert.Equal(6.0, PathCost(path, Cost));
    }

    [Fact]
    public void FindPath_WithAdmissibleHeuristic_MatchesDijkstraCost()
    {
        double Cost((int X, int Y) from, (int X, int Y) to) => ((to.X * 7 + to.Y * 13) % 5) + 1;
        var blocked = new HashSet<(int X, int Y)> { (3, 3), (3, 4), (4, 3), (2, 5), (5, 2) };

        var astar = CreateGridPathfinder(8, 8, blocked, Cost);
        var dijkstra = new AStarPathfinder<(int X, int Y)>(
            neighbors: p => new[] { (p.X + 1, p.Y), (p.X - 1, p.Y), (p.X, p.Y + 1), (p.X, p.Y - 1) }
                            .Where(c => c.Item1 >= 0 && c.Item1 < 8 && c.Item2 >= 0 && c.Item2 < 8 && !blocked.Contains(c)),
            cost: Cost,
            heuristic: (_, _) => 0.0
        );

        var aPath = astar.FindPath((0, 0), (7, 7));
        var dPath = dijkstra.FindPath((0, 0), (7, 7));

        Assert.NotEmpty(aPath);
        Assert.Equal(PathCost(dPath, Cost), PathCost(aPath, Cost), precision: 9);
    }

    [Fact]
    public void FindPath_TinyBudget_ReturnsEmpty_GenerousBudget_Finds()
    {
        var pathfinder = CreateGridPathfinder(50, 50);

        var starved = pathfinder.FindPath((0, 0), (49, 49), maxExpandedNodes: 3);
        var found = pathfinder.TryFindPath((0, 0), (49, 49), out var path, maxExpandedNodes: 100_000);

        Assert.Empty(starved);
        Assert.True(found);
        Assert.NotEmpty(path);
    }

    [Fact]
    public void FindPath_NonPositiveBudget_Throws()
    {
        var pathfinder = CreateGridPathfinder(3, 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => pathfinder.FindPath((0, 0), (2, 2), maxExpandedNodes: 0));
    }

    [Fact]
    public void FindPath_CustomComparer_MatchesNodesInsensitively()
    {
        var edges = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = ["B"],
            ["b"] = ["C"],
            ["c"] = []
        };

        var pathfinder = new AStarPathfinder<string>(
            neighbors: n => edges.TryGetValue(n, out var next) ? next : [],
            cost: (_, _) => 1.0,
            heuristic: (_, _) => 0.0,
            comparer: StringComparer.OrdinalIgnoreCase
        );

        var path = pathfinder.FindPath("A", "c");

        Assert.Equal(3, path.Count);
    }

    [Fact]
    public void FindPath_DirectedNonGridGraph_PicksCheaperBranch()
    {
        var costs = new Dictionary<(string From, string To), double>
        {
            [("A", "B")] = 1, [("B", "D")] = 10,
            [("A", "C")] = 2, [("C", "D")] = 2
        };
        var edges = new Dictionary<string, string[]>
        {
            ["A"] = ["B", "C"], ["B"] = ["D"], ["C"] = ["D"], ["D"] = []
        };

        var pathfinder = new AStarPathfinder<string>(
            neighbors: n => edges[n],
            cost: (from, to) => costs[(from, to)],
            heuristic: (_, _) => 0.0
        );

        var path = pathfinder.FindPath("A", "D");

        Assert.Equal(["A", "C", "D"], path);
    }

    [Fact]
    public void FindPath_IsThreadSafe_AcrossParallelCalls()
    {
        var pathfinder = CreateGridPathfinder(20, 20);

        Parallel.For(0, 200, i =>
        {
            var goal = (19 - i % 5, 19 - i % 7);
            var path = pathfinder.FindPath((0, 0), goal);

            Assert.NotEmpty(path);
            Assert.Equal((0, 0), path[0]);
            Assert.Equal(goal, path[^1]);
        });
    }

    [Fact]
    public void Constructor_NullDelegates_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new AStarPathfinder<int>(null!, (_, _) => 0, (_, _) => 0));
        Assert.Throws<ArgumentNullException>(() => new AStarPathfinder<int>(_ => [], null!, (_, _) => 0));
        Assert.Throws<ArgumentNullException>(() => new AStarPathfinder<int>(_ => [], (_, _) => 0, null!));
    }
}
