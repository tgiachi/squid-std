namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>Non-generic metadata + serialization view of a persisted entity descriptor.</summary>
public interface IPersistenceEntityDescriptor
{
    ushort TypeId { get; }
    string TypeName { get; }
    int SchemaVersion { get; }
    Type EntityType { get; }
    Type KeyType { get; }
}
