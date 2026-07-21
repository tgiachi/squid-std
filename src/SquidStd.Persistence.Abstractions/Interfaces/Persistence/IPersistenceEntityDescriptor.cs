namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>Non-generic metadata + serialization view of a persisted entity descriptor.</summary>
public interface IPersistenceEntityDescriptor
{
    ushort TypeId { get; }

    /// <summary>
    /// The id this entity used before its id became derived, or null when it never had another one.
    /// Drives the snapshot rename and the journal translation that let existing data survive the
    /// change; harmless to leave in place forever once migrated.
    /// </summary>
    /// <remarks>
    /// Defaulted to null so adding it does not break an existing implementation of this interface
    /// outside this repository.
    /// </remarks>
    ushort? LegacyTypeId => null;
    string TypeName { get; }
    int SchemaVersion { get; }
    Type EntityType { get; }
    Type KeyType { get; }
}
