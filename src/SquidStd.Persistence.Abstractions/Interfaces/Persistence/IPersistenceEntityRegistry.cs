namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>Registry of persisted entity descriptors used by snapshot and journal infrastructure.</summary>
public interface IPersistenceEntityRegistry
{
    bool IsFrozen { get; }
    void Freeze();
    void Register<TEntity, TKey>(IPersistenceEntityDescriptor<TEntity, TKey> descriptor) where TKey : notnull;
    IPersistenceEntityDescriptor GetDescriptor(ushort typeId);
    IPersistenceEntityDescriptor<TEntity, TKey> GetDescriptor<TEntity, TKey>() where TKey : notnull;
    IReadOnlyCollection<IPersistenceEntityDescriptor> GetRegisteredDescriptors();
    bool IsRegistered(ushort typeId);
    bool IsRegistered<TEntity, TKey>() where TKey : notnull;
}
