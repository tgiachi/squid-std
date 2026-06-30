using SquidStd.Tests.Actors.Support;

namespace SquidStd.Tests.Actors;

public class ActorOrderingTests
{
    [Fact]
    public async Task Messages_AreProcessedInFifoOrder()
    {
        await using var actor = new ProbeActor();

        for (var i = 0; i < 100; i++)
        {
            await actor.TellAsync(new Append(i.ToString()));
        }

        var log = await actor.AskAsync<GetLog, string>(new());

        Assert.Equal(string.Join(",", Enumerable.Range(0, 100)), log);
    }
}
