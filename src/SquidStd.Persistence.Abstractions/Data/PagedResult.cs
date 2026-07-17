namespace SquidStd.Persistence.Abstractions.Data;

/// <summary>One page of entities, and the total the filter matched before paging.</summary>
/// <param name="Items">The entities on this page, as detached clones.</param>
/// <param name="Total">
/// How many entities the filter matched, before <paramref name="Skip" /> and <paramref name="Take" /> —
/// which is what a caller needs to know how many pages exist. Counting it costs nothing: the filter has
/// already run.
/// </param>
/// <param name="Skip">The offset this page was taken from.</param>
/// <param name="Take">The page size asked for. The page may hold fewer at the end of the results.</param>
public sealed record PagedResult<TEntity>(IReadOnlyList<TEntity> Items, int Total, int Skip, int Take);
