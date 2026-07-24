using SquidStd.Core.Data.EventLoop;
using SquidStd.Services.Core.Services;
using SquidStd.Services.Core.Services.EventLoop;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.EventLoop;

public class EventLoopServiceTests
{
    [Fact]
    public void Tick_DrainsDispatcher_ExecutesPostedActions()
    {
        var dispatcher = new MainThreadDispatcherService();
        var timer = new FakeTimerService();
        using var loop = new EventLoopService(dispatcher, timer, new EventLoopConfig());
        var executed = 0;
        dispatcher.Post(() => executed++);
        dispatcher.Post(() => executed++);

        var work = loop.Tick();

        Assert.Equal(2, executed);
        Assert.Equal(2, work);
        Assert.Equal(1, loop.TickCount);
    }

    [Fact]
    public void Tick_AdvancesTimerWheel()
    {
        var dispatcher = new MainThreadDispatcherService();
        var timer = new FakeTimerService();
        using var loop = new EventLoopService(dispatcher, timer, new EventLoopConfig());

        loop.Tick();

        Assert.Equal(1, timer.TickUpdates);
    }

    [Fact]
    public void Tick_NoWork_ReturnsZero()
    {
        var dispatcher = new MainThreadDispatcherService();
        var timer = new FakeTimerService();
        using var loop = new EventLoopService(dispatcher, timer, new EventLoopConfig());

        var work = loop.Tick();

        Assert.Equal(0, work);
    }

    [Fact]
    public async Task Collect_ReturnsEventLoopMetrics()
    {
        var dispatcher = new MainThreadDispatcherService();
        var timer = new FakeTimerService();
        using var loop = new EventLoopService(dispatcher, timer, new EventLoopConfig());
        loop.Tick();

        var samples = await loop.CollectAsync();

        Assert.Equal("eventloop", loop.ProviderName);
        Assert.Contains(samples, s => s.Name == "tick_count");
        Assert.Contains(samples, s => s.Name == "tick_avg_ms");
        Assert.Contains(samples, s => s.Name == "tick_max_ms");
        Assert.Contains(samples, s => s.Name == "idle_sleeps_total");
    }

    [Fact]
    public async Task StartStop_ExecutesPostedWork()
    {
        var dispatcher = new MainThreadDispatcherService();
        var timer = new FakeTimerService();
        using var loop = new EventLoopService(dispatcher, timer, new EventLoopConfig());
        var signal = new ManualResetEventSlim(false);
        dispatcher.Post(() => signal.Set());

        await loop.StartAsync();
        var fired = signal.Wait(TimeSpan.FromSeconds(2));
        await loop.StopAsync();

        Assert.True(fired);
    }

    [Fact]
    public async Task IsOnLoopThread_TrueInsidePostedWork_FalseOutside()
    {
        var dispatcher = new MainThreadDispatcherService();
        var timer = new FakeTimerService();
        using var loop = new EventLoopService(dispatcher, timer, new EventLoopConfig());

        Assert.False(loop.IsOnLoopThread);

        bool onLoopInside = false;
        var signal = new ManualResetEventSlim(false);
        dispatcher.Post(() =>
            {
                onLoopInside = loop.IsOnLoopThread;
                signal.Set();
            }
        );

        await loop.StartAsync();
        signal.Wait(TimeSpan.FromSeconds(2));
        await loop.StopAsync();

        Assert.True(onLoopInside);
        Assert.False(loop.IsOnLoopThread);
    }
}
