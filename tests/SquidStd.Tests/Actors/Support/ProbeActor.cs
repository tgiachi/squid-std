using SquidStd.Actors;
using SquidStd.Actors.Data;

namespace SquidStd.Tests.Actors.Support;

/// <summary>Configurable actor that records processed values and surfaced errors for assertions.</summary>
public sealed class ProbeActor : Actor<IProbeMessage>
{
    private readonly List<string> _log = new();

    /// <summary>Error messages captured by <see cref="OnErrorAsync" />.</summary>
    public List<string> Errors { get; } = new();

    public ProbeActor(ActorOptions? options = null)
        : base(options) { }

    protected override async ValueTask ReceiveAsync(IProbeMessage message, CancellationToken cancellationToken)
    {
        switch (message)
        {
            case Append append:
                _log.Add(append.Value);

                break;
            case Boom:
                throw new InvalidOperationException("boom");
            case Hold hold:
                await hold.Gate.Task;

                break;
            case HoldUntilCancelled:
                await Task.Delay(Timeout.Infinite, cancellationToken);

                break;
            case GetLog getLog:
                getLog.Reply(string.Join(",", _log));

                break;
            case FailingRequest:
                throw new InvalidOperationException("ask-boom");
        }
    }

    protected override ValueTask OnErrorAsync(IProbeMessage message, Exception error)
    {
        Errors.Add(error.Message);

        return ValueTask.CompletedTask;
    }
}
