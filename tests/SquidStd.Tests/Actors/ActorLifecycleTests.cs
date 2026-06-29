using SquidStd.Actors.Data;
using SquidStd.Tests.Actors.Support;

namespace SquidStd.Tests.Actors;

public class ActorLifecycleTests
{
    [Fact]
    public async Task DisposeAsync_DrainsQueuedMessages()
    {
        var gate = new TaskCompletionSource();
        var actor = new ProbeActor();
        await actor.TellAsync(new Hold(gate)); // in-flight, blocks until released (ignores cancellation)
        await actor.TellAsync(new Append("a")); // queued behind the hold
        await actor.TellAsync(new Append("b")); // queued behind the hold
        var logTask = actor.AskAsync<GetLog, string>(new GetLog()); // queued last
        await Task.Delay(50); // let the request enqueue before dispose completes the mailbox

        var disposeTask = actor.DisposeAsync();
        gate.SetResult(); // release the hold so the queue can drain

        var log = await logTask;
        await disposeTask;

        Assert.Equal("a,b", log); // the queued Tells ran during dispose instead of being dropped
    }

    [Fact]
    public async Task DisposeAsync_DrainTimeout_FaultsOutstandingAsk()
    {
        var gate = new TaskCompletionSource();
        var actor = new ProbeActor(new ActorOptions { ShutdownDrainTimeout = TimeSpan.FromMilliseconds(50) });
        await actor.TellAsync(new Hold(gate)); // in-flight, never released and ignores cancellation
        var ask = actor.AskAsync<GetLog, string>(new GetLog()); // queued behind, never reached
        await Task.Delay(50);

        await actor.DisposeAsync(); // drain exceeds the budget; the still-pending request is faulted

        await Assert.ThrowsAsync<ObjectDisposedException>(() => ask);

        gate.SetResult(); // release the orphaned handler (Reply is idempotent)
    }

    [Fact]
    public async Task TellAsync_AfterDispose_Throws()
    {
        var actor = new ProbeActor();
        await actor.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await actor.TellAsync(new Append("x"))
        );
    }

    [Fact]
    public async Task DisposeAsync_CalledConcurrentlyTwice_IsIdempotent()
    {
        var actor = new ProbeActor();

        // The dispose guard is claimed atomically, so concurrent disposers must not both run teardown.
        var first = actor.DisposeAsync();
        var second = actor.DisposeAsync();

        await first;
        await second;
    }
}
