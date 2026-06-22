namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
/// Internal mutable timer wheel entry.
/// </summary>
internal sealed class TimerEntry
{
    public required Action Callback { get; init; }
    public required string Id { get; init; }
    public required TimeSpan Interval { get; init; }
    public required string Name { get; init; }
    public required bool Repeat { get; init; }

    public bool Cancelled { get; set; }
    public LinkedListNode<TimerEntry>? Node { get; set; }
    public long RemainingRounds { get; set; }
    public int SlotIndex { get; set; }
}
