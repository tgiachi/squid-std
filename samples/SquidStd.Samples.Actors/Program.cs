using SquidStd.Actors;

await using var counter = new CounterActor();

#region step-2

// Fire-and-forget messages: TellAsync enqueues without awaiting a reply.
await counter.TellAsync(new Increment(5));
await counter.TellAsync(new Increment(3));

#endregion

#region step-3

// Request/response: AskAsync enqueues a request and awaits its typed reply.
var total = await counter.AskAsync<GetTotal, int>(new());

Console.WriteLine($"Total: {total}");

#endregion

#region step-1

// The message contract: a marker interface, a fire-and-forget message, and an ask request.
internal interface ICounterMessage;

internal sealed record Increment(int By) : ICounterMessage;

internal sealed record GetTotal : ActorRequest<int>, ICounterMessage;

// A single-consumer actor: state is mutated without locks inside ReceiveAsync.
internal sealed class CounterActor : Actor<ICounterMessage>
{
    private int _total;

    protected override ValueTask ReceiveAsync(ICounterMessage message, CancellationToken cancellationToken)
    {
        switch (message)
        {
            case Increment increment:
                _total += increment.By;

                break;
            case GetTotal request:
                request.Reply(_total);

                break;
        }

        return ValueTask.CompletedTask;
    }
}

#endregion
