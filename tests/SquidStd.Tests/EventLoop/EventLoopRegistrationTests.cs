using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Core.Interfaces.Threading;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services;
using SquidStd.Services.Core.Services.Scheduling;

namespace SquidStd.Tests.EventLoop;

public class EventLoopRegistrationTests
{
    [Fact]
    public void RegisterEventLoop_RegistersDriverAndMetricProvider()
    {
        using var container = new Container();
        container.RegisterCoreServices("squidstd", Path.GetTempPath());

        container.RegisterEventLoop();

        Assert.True(container.IsRegistered<IEventLoopService>());
        Assert.True(container.IsRegistered<ITimerWheelDriver>());
    }

    [Fact]
    public void RegisterEventLoop_AfterPump_Throws()
    {
        using var container = new Container();
        container.RegisterCoreServices("squidstd", Path.GetTempPath());
        container.RegisterConfigSection("timerWheelPump", static () => new SquidStd.Core.Data.Timing.TimerWheelPumpConfig(), -90);
        container.RegisterStdService<TimerWheelPumpService, TimerWheelPumpService>(-1);
        container.RegisterMapping<ITimerWheelDriver, TimerWheelPumpService>();

        Assert.Throws<InvalidOperationException>(() => container.RegisterEventLoop());
    }

    [Fact]
    public void EventLoopService_IsATimerWheelDriver()
    {
        Assert.True(typeof(ITimerWheelDriver).IsAssignableFrom(typeof(SquidStd.Services.Core.Services.EventLoop.EventLoopService)));
    }

    [Fact]
    public void TimerWheelPumpService_IsATimerWheelDriver()
    {
        Assert.True(typeof(ITimerWheelDriver).IsAssignableFrom(typeof(TimerWheelPumpService)));
    }
}
