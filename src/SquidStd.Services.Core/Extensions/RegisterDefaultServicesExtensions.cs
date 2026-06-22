using DryIoc;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Core.Data.Jobs;
using SquidStd.Core.Data.Timing;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Core.Interfaces.Jobs;
using SquidStd.Core.Interfaces.Threading;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Services.Core.Services;

namespace SquidStd.Services.Core.Extensions;

/// <summary>
/// Extension methods for registering the default SquidStd core services.
/// </summary>
public static class RegisterDefaultServicesExtensions
{
    /// <param name="container">Container that receives the core service registrations.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the default SquidStd core services in the container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterCoreServices()
        {
            container.RegisterEventBusService();
            container.RegisterJobSystemService();
            container.RegisterMainThreadDispatcherService();
            container.RegisterTimerWheelService();

            return container;
        }

        /// <summary>
        /// Registers the default event bus service in the container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterEventBusService()
        {
            return container.RegisterStdService<IEventBus, EventBusService>(-1);
        }

        /// <summary>
        /// Registers the default job system service in the container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterJobSystemService()
        {
            if (!container.IsRegistered<JobsConfig>())
            {
                container.RegisterInstance(new JobsConfig());
            }

            return container.RegisterStdService<IJobSystem, JobSystemService>(-1);
        }

        /// <summary>
        /// Registers the default main-thread dispatcher service in the container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterMainThreadDispatcherService()
        {
            return container.RegisterStdService<IMainThreadDispatcher, MainThreadDispatcherService>(-1);
        }

        /// <summary>
        /// Registers the default timer wheel service in the container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterTimerWheelService()
        {
            if (!container.IsRegistered<TimerWheelConfig>())
            {
                container.RegisterInstance(new TimerWheelConfig());
            }

            return container.RegisterStdService<ITimerService, TimerWheelService>(-1);
        }
    }
}
