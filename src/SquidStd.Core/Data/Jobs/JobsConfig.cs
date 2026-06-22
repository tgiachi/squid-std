namespace SquidStd.Core.Data.Jobs;

/// <summary>
/// Configuration for the background job system.
/// </summary>
public sealed class JobsConfig
{
    /// <summary>
    /// Gets or sets the number of worker threads.
    /// </summary>
    public int WorkerThreadCount { get; set; }

    /// <summary>
    /// Gets or sets the seconds to wait for worker shutdown.
    /// </summary>
    public double ShutdownTimeoutSeconds { get; set; } = 1.0;
}
