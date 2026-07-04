namespace SquidStd.Core.Buffers;

/// <summary>
/// Debug-only bookkeeping record tracking whether a pooled array is currently rented, used by
/// <see cref="STArrayPool{T}" /> under the <c>DEBUG_ARRAYPOOL</c> conditional to detect double-returns.
/// </summary>
internal sealed class STArrayPoolRentReturnStatus
{
    /// <summary>The stack trace captured at the most recent rent or return.</summary>
    public string? StackTrace { get; set; }

    /// <summary>Whether the array is currently considered rented.</summary>
    public bool IsRented { get; set; }
}
