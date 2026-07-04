namespace SquidStd.Core.Buffers;

/// <summary>
/// A single cached array slot with a last-seen timestamp, used by <see cref="STArrayPool{T}" /> to
/// remember the most recently returned array for a given bucket size.
/// </summary>
internal struct STArrayPoolBucket<T>
{
    /// <summary>The cached array, or <see langword="null" /> if the slot is empty.</summary>
    public T[]? Array;

    /// <summary>The tick count (in milliseconds) at which the array was last observed idle by trimming.</summary>
    public long Ticks;

    /// <summary>
    /// Initializes a new instance of the <see cref="STArrayPoolBucket{T}" /> struct wrapping the given array.
    /// </summary>
    /// <param name="array">The array to cache.</param>
    public STArrayPoolBucket(T[] array)
    {
        Array = array;
        Ticks = 0;
    }
}
