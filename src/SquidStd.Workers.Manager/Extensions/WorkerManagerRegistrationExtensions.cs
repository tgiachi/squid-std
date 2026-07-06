using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Core.Data.Timing;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Services.Core.Services.Scheduling;
using SquidStd.Workers.Manager.Data.Config;
using SquidStd.Workers.Manager.Interfaces;
using SquidStd.Workers.Manager.Services;

namespace SquidStd.Workers.Manager.Extensions;

/// <summary>
/// DryIoc registration helpers for the worker manager.
/// </summary>
public static class WorkerManagerRegistrationExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the worker manager: config section, registry, job scheduler, the collector and sweep
        /// lifecycle services, and the timer-wheel pump (only if it is not already registered).
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        public IContainer AddWorkerManager(WorkerManagerConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(container);

            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("workerManager", static () => new WorkerManagerConfig(), -50);
            }

            container.Register<WorkerRegistry>(Reuse.Singleton);
            container.RegisterMapping<IWorkerRegistry, WorkerRegistry>();
            container.Register<IJobScheduler, JobScheduler>(Reuse.Singleton);

            container.RegisterStdService<HeartbeatCollectorService, HeartbeatCollectorService>(100);
            container.RegisterStdService<WorkerOfflineSweepService, WorkerOfflineSweepService>(110);

            if (!container.IsRegistered<ITimerWheelDriver>())
            {
                container.RegisterConfigSection("timerWheelPump", static () => new TimerWheelPumpConfig(), -90);
                container.RegisterStdService<TimerWheelPumpService, TimerWheelPumpService>(-1);
                container.RegisterMapping<ITimerWheelDriver, TimerWheelPumpService>();
            }

            return container;
        }
    }
}
