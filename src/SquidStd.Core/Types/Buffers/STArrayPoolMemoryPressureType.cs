namespace SquidStd.Core.Types.Buffers;

/// <summary>
/// GC memory pressure levels used to decide how aggressively <see cref="Buffers.STArrayPool{T}" /> trims.
/// </summary>
internal enum STArrayPoolMemoryPressureType
{
    /// <summary>Memory load is well below the high-load threshold.</summary>
    Low,

    /// <summary>Memory load is approaching the high-load threshold.</summary>
    Medium,

    /// <summary>Memory load is at or above the high-load threshold.</summary>
    High
}
