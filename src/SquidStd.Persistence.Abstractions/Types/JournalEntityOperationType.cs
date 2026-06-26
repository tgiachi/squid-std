namespace SquidStd.Persistence.Abstractions.Types;

/// <summary>Identifies a journal mutation for a registered entity type.</summary>
public enum JournalEntityOperationType : byte
{
    Upsert = 1,
    Remove = 2
}
