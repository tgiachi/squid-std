namespace SquidStd.Search.Abstractions.Interfaces;

/// <summary>
///     Marks an entity as indexable and supplies its document id. The target index comes from a
///     <see cref="Attributes.SearchIndexAttribute" /> on the type (or the lowercased type name).
/// </summary>
public interface IIndexableEntity
{
    /// <summary>Stable document id within the index.</summary>
    string IndexId { get; }
}
