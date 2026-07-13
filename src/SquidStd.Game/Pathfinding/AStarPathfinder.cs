using System.Diagnostics;
using SquidStd.Core.Collections.PriorityQueues;

namespace SquidStd.Game.Pathfinding;

/// <summary>
/// Generic A* pathfinder over any graph: the caller supplies the successors of a node, the
/// edge cost, and the goal heuristic as delegates, so any node type works. A consistent
/// (monotone) heuristic - one that never decreases by more than the edge cost between
/// adjacent nodes, which includes all common geometric distances and the zero heuristic -
/// guarantees an optimal path; a zero heuristic turns the search into Dijkstra. Instances
/// hold no search state - every <see cref="FindPath" /> call keeps its own - so a single
/// pathfinder is thread-safe and reusable across concurrent searches, provided the supplied
/// delegates and comparer are themselves safe for concurrent invocation.
/// </summary>
/// <typeparam name="TNode">The graph node type.</typeparam>
public sealed class AStarPathfinder<TNode> where TNode : notnull
{
    private const int InitialOpenSetCapacity = 64;

    private readonly IEqualityComparer<TNode> _comparer;
    private readonly Func<TNode, TNode, double> _cost;
    private readonly Func<TNode, TNode, double> _heuristic;
    private readonly Func<TNode, IEnumerable<TNode>> _neighbors;

    /// <summary>
    /// Initializes the pathfinder with the graph delegates.
    /// </summary>
    /// <param name="neighbors">
    /// Returns the reachable successors of a node. Must never return null - an empty
    /// sequence means no successors.
    /// </param>
    /// <param name="cost">
    /// Edge cost from a node to a successor. Must be non-negative; negative costs are
    /// undefined behavior and only checked in DEBUG builds.
    /// </param>
    /// <param name="heuristic">
    /// Estimated remaining cost from a node to the goal. Consistent (monotone) estimates
    /// yield optimal paths; return 0 for Dijkstra behavior.
    /// </param>
    /// <param name="comparer">Node equality comparer; defaults to <see cref="EqualityComparer{T}.Default" />.</param>
    public AStarPathfinder(
        Func<TNode, IEnumerable<TNode>> neighbors,
        Func<TNode, TNode, double> cost,
        Func<TNode, TNode, double> heuristic,
        IEqualityComparer<TNode>? comparer = null
    )
    {
        ArgumentNullException.ThrowIfNull(neighbors);
        ArgumentNullException.ThrowIfNull(cost);
        ArgumentNullException.ThrowIfNull(heuristic);

        _neighbors = neighbors;
        _cost = cost;
        _heuristic = heuristic;
        _comparer = comparer ?? EqualityComparer<TNode>.Default;
    }

    /// <summary>
    /// Finds the cheapest path from <paramref name="start" /> to <paramref name="goal" />.
    /// The result includes both endpoints; it is empty (never null) when the goal is
    /// unreachable or the expansion budget runs out, and contains only the start node when
    /// start equals goal.
    /// </summary>
    /// <param name="start">The start node.</param>
    /// <param name="goal">The goal node.</param>
    /// <param name="maxExpandedNodes">
    /// Optional per-call budget: the maximum number of nodes to expand before giving up.
    /// Protects real-time callers from unbounded searches toward unreachable goals.
    /// </param>
    /// <returns>The path, or an empty list.</returns>
    public IReadOnlyList<TNode> FindPath(TNode start, TNode goal, int? maxExpandedNodes = null)
    {
        if (maxExpandedNodes.HasValue && maxExpandedNodes.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExpandedNodes));
        }

        if (_comparer.Equals(start, goal))
        {
            return [start];
        }

        var open = new GenericPriorityQueue<SearchNode, double>(InitialOpenSetCapacity);
        var nodes = new Dictionary<TNode, SearchNode>(_comparer);

        var startNode = new SearchNode(start) { GScore = 0.0, HScore = _heuristic(start, goal) };
        nodes[start] = startNode;
        open.Enqueue(startNode, startNode.HScore);

        var expanded = 0;

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            current.Expanded = true;
            expanded++;

            if (_comparer.Equals(current.Value, goal))
            {
                return Reconstruct(current);
            }

            if (expanded >= maxExpandedNodes)
            {
                return [];
            }

            foreach (var neighbor in _neighbors(current.Value))
            {
                var edgeCost = _cost(current.Value, neighbor);
                Debug.Assert(edgeCost >= 0.0, "A* requires non-negative edge costs.");

                var tentative = current.GScore + edgeCost;

                if (nodes.TryGetValue(neighbor, out var known))
                {
                    if (known.Expanded || tentative >= known.GScore)
                    {
                        continue;
                    }

                    known.GScore = tentative;
                    known.Parent = current;
                    open.UpdatePriority(known, tentative + known.HScore);
                }
                else
                {
                    var created = new SearchNode(neighbor)
                    {
                        GScore = tentative, HScore = _heuristic(neighbor, goal), Parent = current
                    };
                    nodes[neighbor] = created;

                    if (open.Count == open.MaxSize)
                    {
                        open.Resize(open.MaxSize * 2);
                    }

                    open.Enqueue(created, tentative + created.HScore);
                }
            }
        }

        return [];
    }

    /// <summary>
    /// Try-variant of <see cref="FindPath" />: returns false (with an empty path) when the
    /// goal is unreachable or the expansion budget runs out.
    /// </summary>
    /// <param name="start">The start node.</param>
    /// <param name="goal">The goal node.</param>
    /// <param name="path">The found path, or an empty list.</param>
    /// <param name="maxExpandedNodes">Optional per-call expansion budget.</param>
    /// <returns>True when a path was found.</returns>
    public bool TryFindPath(TNode start, TNode goal, out IReadOnlyList<TNode> path, int? maxExpandedNodes = null)
    {
        path = FindPath(start, goal, maxExpandedNodes);

        return path.Count > 0;
    }

    private static IReadOnlyList<TNode> Reconstruct(SearchNode goalNode)
    {
        var path = new List<TNode>();

        for (var node = goalNode; node is not null; node = node.Parent)
        {
            path.Add(node.Value);
        }

        path.Reverse();

        return path;
    }

    private sealed class SearchNode : GenericPriorityQueueNode<double>
    {
        public bool Expanded { get; set; }

        public double GScore { get; set; }

        public double HScore { get; set; }

        public SearchNode? Parent { get; set; }

        public TNode Value { get; }

        public SearchNode(TNode value)
        {
            Value = value;
        }
    }
}
