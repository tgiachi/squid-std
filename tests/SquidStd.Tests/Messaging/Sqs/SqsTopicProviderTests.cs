using System.Text;
using SquidStd.Messaging.Sqs.Data.Config;
using SquidStd.Messaging.Sqs.Services;

namespace SquidStd.Tests.Messaging.Sqs;

[Collection(LocalStackCollection.Name)]
public class SqsTopicProviderTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    private readonly LocalStackContainerFixture _fixture;

    public SqsTopicProviderTests(LocalStackContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Publish_FansOutToAllSubscribers()
    {
        await using var provider = NewProvider();
        await provider.StartAsync();
        var topic = Topic();
        var a = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var b = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.Subscribe(
            topic,
            (payload, _) =>
            {
                a.TrySetResult(Text(payload));
                return Task.CompletedTask;
            }
        );
        provider.Subscribe(
            topic,
            (payload, _) =>
            {
                b.TrySetResult(Text(payload));
                return Task.CompletedTask;
            }
        );

        // Allow the subscriptions to be wired before publishing.
        await Task.Delay(2000);
        await provider.PublishAsync(topic, Bytes("hello"));

        Assert.Equal("hello", await a.Task.WaitAsync(Timeout));
        Assert.Equal("hello", await b.Task.WaitAsync(Timeout));
    }

    private SqsTopicProvider NewProvider()
    {
        return new SqsTopicProvider(new SqsOptions { Aws = _fixture.Aws, WaitTimeSeconds = 1 });
    }

    private static ReadOnlyMemory<byte> Bytes(string s)
    {
        return Encoding.UTF8.GetBytes(s);
    }

    private static string Text(ReadOnlyMemory<byte> b)
    {
        return Encoding.UTF8.GetString(b.Span);
    }

    private static string Topic()
    {
        return "topic-" + Guid.NewGuid().ToString("N");
    }
}
