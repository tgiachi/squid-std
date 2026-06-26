namespace SquidStd.Search.Abstractions.Interfaces;

/// <summary>
///     Indexes, deletes, and queries <see cref="IIndexableEntity" /> documents.
/// </summary>
public interface ISearchService
{
    /// <summary>Deletes a document by id. Returns <c>false</c> when it did not exist.</summary>
    Task<bool> DeleteAsync<T>(string indexId, bool refresh = false, CancellationToken cancellationToken = default)
        where T : IIndexableEntity;

    /// <summary>Creates the index for <typeparamref name="T" /> if it does not exist (dynamic mapping). Idempotent.</summary>
    Task EnsureIndexAsync<T>(CancellationToken cancellationToken = default) where T : IIndexableEntity;

    /// <summary>Indexes (creates or replaces) a single document.</summary>
    Task IndexAsync<T>(T entity, bool refresh = false, CancellationToken cancellationToken = default)
        where T : IIndexableEntity;

    /// <summary>Indexes many documents in one bulk request.</summary>
    Task IndexManyAsync<T>(IEnumerable<T> entities, bool refresh = false, CancellationToken cancellationToken = default)
        where T : IIndexableEntity;

    /// <summary>Returns a constrained <see cref="IQueryable{T}" /> translated to the Elasticsearch query DSL.</summary>
    IQueryable<T> Query<T>() where T : IIndexableEntity;
}
