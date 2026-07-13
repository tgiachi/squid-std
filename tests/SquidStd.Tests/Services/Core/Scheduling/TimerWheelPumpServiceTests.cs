using SquidStd.Services.Core.Services.Scheduling;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Services.Core.Scheduling;

public class TimerWheelPumpServiceTests
{
    [Fact]
    public void Ctor_NonPositiveInterval_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => new TimerWheelPumpService(
                new FakeTimerService(),
                new() { PumpInterval = TimeSpan.Zero }
            )
        );

    [Fact]
    public async Task Pump_AdvancesTheWheel()
    {
        var timer = new FakeTimerService();
        var pump = new TimerWheelPumpService(
            timer,
            new() { PumpInterval = TimeSpan.FromMilliseconds(20) }
        );

        await pump.StartAsync();

        // The pump loop is dispatched via Task.Run; a generous bound absorbs thread-pool
        // scheduling delay on loaded CI runners while still failing fast if it never pumps.
        Assert.True(timer.Pumped.Wait(TimeSpan.FromSeconds(30)));

        await pump.StopAsync();
        pump.Dispose();

        Assert.True(timer.TickUpdates >= 1);
    }
}
