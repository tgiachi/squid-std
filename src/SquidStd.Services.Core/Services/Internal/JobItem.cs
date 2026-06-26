namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
///     Internal envelope for a scheduled job.
/// </summary>
internal sealed class JobItem
{
    private readonly Action _cancel;
    private readonly Action _run;

    public JobItem(Action run, Action cancel)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(cancel);
        _run = run;
        _cancel = cancel;
    }

    public void Cancel()
    {
        _cancel();
    }

    public void Run()
    {
        _run();
    }
}
