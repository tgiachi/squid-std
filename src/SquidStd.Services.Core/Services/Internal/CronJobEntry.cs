using Cronos;

namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
///     Internal mutable state for a registered cron job.
/// </summary>
internal sealed class CronJobEntry
{
    public DateTime? LastRunUtc;
    public DateTime? NextOccurrenceUtc;
    public long RunCount;
    public int Running;

    public string? TimerId;
    public required string JobId { get; init; }
    public required string Name { get; init; }
    public required string CronText { get; init; }
    public required CronExpression Expression { get; init; }
    public required Func<CancellationToken, Task> Handler { get; init; }
}
