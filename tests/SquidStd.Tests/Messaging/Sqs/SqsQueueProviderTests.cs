using System.Text;
using SquidStd.Messaging.Abstractions.Data.Config;
using SquidStd.Messaging.Sqs.Data.Config;
using SquidStd.Messaging.Sqs.Services;

namespace SquidStd.Tests.Messaging.Sqs;

[Collection(LocalStackCollection.Name)]
public class SqsQueueProviderTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    private readonly LocalStackContainerFixture _fixture;

    public SqsQueueProviderTests(LocalStackContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Publish_DeliversToSubscriber()
    {
        await using var provider = NewProvider();
        await provider.StartAsync();
        var queue = Queue();
        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.Subscribe(
            queue,
            (payload, _) =>
            {
                received.TrySetResult(Text(payload));

                return Task.CompletedTask;
            }
        );

        await provider.PublishAsync(queue, Bytes("hello"));

        Assert.Equal("hello", await received.Task.WaitAsync(Timeout));
    }

    [Fact]
    public async Task TwoSubscribers_ShareTheLoad()
    {
        await using var provider = NewProvider();
        await provider.StartAsync();
        var queue = Queue();
        const int total = 10;
        var a = 0;
        var b = 0;
        using var done = new CountdownEvent(total);
        provider.Subscribe(
            queue,
            (_, _) =>
            {
                Interlocked.Increment(ref a);
                done.Signal();
                return Task.CompletedTask;
            }
        );
        provider.Subscribe(
            queue,
            (_, _) =>
            {
                Interlocked.Increment(ref b);
                done.Signal();
                return Task.CompletedTask;
            }
        );

        for (var i = 0; i < total; i++)
        {
            await provider.PublishAsync(queue, Bytes("m"));
        }

        Assert.True(done.Wait(Timeout));
        Assert.Equal(total, a + b);
    }

    [Fact]
    public async Task AlwaysFailing_IsDeadLettered()
    {
        await using var provider = NewProvider(
            new MessagingOptions { MaxDeliveryAttempts = 1 },
            new SqsOptions { Aws = _fixture.Aws, VisibilityTimeout = TimeSpan.FromSeconds(1), WaitTimeSeconds = 1 }
        );
        await provider.StartAsync();
        var queue = Queue();
        provider.Subscribe(queue, (_, _) => throw new InvalidOperationException("always"));

        var deadLettered = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.Subscribe(
            queue + "-dlq",
            (payload, _) =>
            {
                deadLettered.TrySetResult(Text(payload));
                return Task.CompletedTask;
            }
        );

        await provider.PublishAsync(queue, Bytes("poison"));

        Assert.Equal("poison", await deadLettered.Task.WaitAsync(Timeout));
    }

    private SqsQueueProvider NewProvider(MessagingOptions? messaging = null, SqsOptions? sqs = null)
    {
        return new SqsQueueProvider(
            sqs ?? new SqsOptions { Aws = _fixture.Aws, WaitTimeSeconds = 1 },
            messaging ?? new MessagingOptions()
        );
    }

    private static ReadOnlyMemory<byte> Bytes(string s)
    {
        return Encoding.UTF8.GetBytes(s);
    }

    private static string Text(ReadOnlyMemory<byte> b)
    {
        return Encoding.UTF8.GetString(b.Span);
    }

    private static string Queue()
    {
        return "q-" + Guid.NewGuid().ToString("N");
    }
}
