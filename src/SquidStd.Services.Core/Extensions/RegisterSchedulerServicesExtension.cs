using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Core.Data.EventLoop;
using SquidStd.Core.Data.Timing;
using SquidStd.Core.Interfaces.Metrics;
using SquidStd.Core.Interfaces.Scheduling;
using SquidStd.Core.Interfaces.Threading;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Services.Core.Services.EventLoop;
using SquidStd.Services.Core.Services.Scheduling;

namespace SquidStd.Services.Core.Extensions;

/// <summary>
/// Extension methods for registering the SquidStd cron scheduler.
/// </summary>
public static class RegisterSchedulerServicesExtension
{
    /// <param name="container">Container that receives the scheduler registrations.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the timer wheel pump and the cron scheduler. Must be called after
        /// <c>RegisterCoreServices</c> so that <c>ITimerService</c> and <c>IJobSystem</c> exist.
        /// </summary>
        /// <param name="pumpConfig">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterSchedulerServices(TimerWheelPumpConfig? pumpConfig = null)
        {
            if (pumpConfig is not null)
            {
                container.RegisterInstance(pumpConfig, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("timerWheelPump", static () => new TimerWheelPumpConfig(), -90);
            }

            container.RegisterStdService<TimerWheelPumpService, TimerWheelPumpService>(-1);
            container.RegisterMapping<ITimerWheelDriver, TimerWheelPumpService>();
            container.RegisterStdService<ICronScheduler, CronSchedulerService>(-1);

            return container;
        }

        /// <summary>
        /// Registers the event loop (dedicated thread draining the dispatcher and advancing the timer
        /// wheel). Mutually exclusive with the timer-wheel pump: throws if a timer-wheel driver is
        /// already registered. Must be called after <c>RegisterCoreServices</c> so that
        /// <c>IMainThreadDispatcher</c> and <c>ITimerService</c> exist.
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterEventLoop(EventLoopConfig? config = null)
        {
            if (container.IsRegistered<ITimerWheelDriver>())
            {
                throw new InvalidOperationException(
                    "A timer wheel driver is already registered (EventLoop or TimerWheelPump). They are mutually exclusive."
                );
            }

            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("eventLoop", static () => new EventLoopConfig(), -90);
            }

            container.RegisterStdService<IEventLoopService, EventLoopService>(-1);
            container.RegisterMapping<ITimerWheelDriver, IEventLoopService>();
            container.RegisterMapping<IMetricProvider, IEventLoopService>();

            return container;
        }
    }
}
