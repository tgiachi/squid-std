<h1 align="center">SquidStd.Game</h1>

Small, dependency-light building blocks for games: dice-notation rolling and a generic A*
pathfinder. Both are pure logic - no engine, no renderer, no fixed grid type - so they drop into
any turn-based or real-time game loop built on SquidStd.

## Install

```bash
dotnet add package SquidStd.Game
```

## Usage

```csharp
using SquidStd.Game.Dice;

// Parse dice notation: "[N]dS[+/-M]" (for example "2d6+1", "d20"), the wrapped
// "dice(2d6+1)" form, or a pure constant such as "5".
var expression = DiceExpression.Parse("2d6+1");

expression.Min;     // 3  - every die shows 1
expression.Max;     // 13 - every die shows its max face
expression.Average; // 8.0

var total = expression.Roll(); // random total in [Min, Max]
```

## Pathfinding

```csharp
using SquidStd.Game.Pathfinding;

const int width = 20;
const int height = 20;
var blocked = new HashSet<(int X, int Y)> { (5, 5), (5, 6), (5, 7) };

var pathfinder = new AStarPathfinder<(int X, int Y)>(
    neighbors: p => new (int X, int Y)[] { (p.X + 1, p.Y), (p.X - 1, p.Y), (p.X, p.Y + 1), (p.X, p.Y - 1) }
        .Where(c => c.X >= 0 && c.X < width && c.Y >= 0 && c.Y < height && !blocked.Contains(c)),
    cost: (_, _) => 1.0,
    heuristic: (p, goal) => Math.Abs(p.X - goal.X) + Math.Abs(p.Y - goal.Y) // Manhattan distance
);

var path = pathfinder.FindPath((0, 0), (10, 10), maxExpandedNodes: 5_000);

// Or the try-variant, which reports success instead of checking for an empty list:
var found = pathfinder.TryFindPath((0, 0), (10, 10), out var tryPath, maxExpandedNodes: 5_000);
```

The returned path includes both the start and the goal node. It is empty (never null) when the
goal is unreachable or the expansion budget runs out, and contains only the start node when start
equals goal. A consistent (monotone) heuristic - one that never decreases by more than the edge
cost between adjacent nodes, which includes Manhattan/Euclidean distance and the zero heuristic -
guarantees an optimal path; a zero heuristic turns the search into Dijkstra. The optional
`maxExpandedNodes` budget caps the work done per call so real-time callers (game loops) don't stall
chasing an unreachable goal. A pathfinder instance holds no search state of its own - every
`FindPath` call keeps its own - so a single instance is thread-safe and reusable across concurrent
searches, provided the supplied delegates and comparer are themselves safe for concurrent
invocation.

## Key types

| Type                      | Purpose                                                             |
|---------------------------|-----------------------------------------------------------------------|
| `DiceExpression`          | Parsed `[N]dS[+/-M]` dice notation with `Min`/`Max`/`Average`/`Roll`. |
| `AStarPathfinder<TNode>`  | Generic A* over any graph via neighbor/cost/heuristic delegates.      |

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
