using SquidStd.Actors;

namespace SquidStd.Tests.Actors;

public class ActorAskTests
{
    private sealed record SampleRequest : ActorRequest<int>;

    [Fact]
    public async Task Reply_CompletesTheCompletionTask()
    {
        var request = new SampleRequest();

        request.Reply(42);

        Assert.True(request.Completion.IsCompletedSuccessfully);
        Assert.Equal(42, await request.Completion);
    }

    [Fact]
    public async Task Fail_FaultsTheCompletionTask()
    {
        var request = new SampleRequest();

        request.Fail(new InvalidOperationException("nope"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Completion);
        Assert.Equal("nope", ex.Message);
    }
}
