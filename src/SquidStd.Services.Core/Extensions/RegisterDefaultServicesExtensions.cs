using DryIoc;
using SquidStd.Abstractions.Data.Internal.Services;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Container;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Core.Config;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Data.Events;
using SquidStd.Core.Data.Jobs;
using SquidStd.Core.Data.Metrics;
using SquidStd.Core.Data.Storage;
using SquidStd.Core.Data.Timing;
using SquidStd.Core.Directories;
using SquidStd.Core.Files;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Core.Interfaces.Files;
using SquidStd.Core.Interfaces.Jobs;
using SquidStd.Core.Interfaces.Metrics;
using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Core.Interfaces.Threading;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Core.Json;
using SquidStd.Core.Types.Yaml;
using SquidStd.Core.Yaml;
using SquidStd.Services.Core.Services;
using SquidStd.Services.Core.Services.Storage;

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
        /// Registers the default config manager service as a singleton instance, loading a
        /// standalone <see cref="SquidStdConfig" /> for the given name/directory. Any subsequent
        /// <c>RegisterConfigSection</c> call on this container binds eagerly against it.
        /// </summary>
        /// <param name="configName">The logical config name or YAML file name.</param>
        /// <param name="configDirectory">The directory where the config file is searched.</param>
        /// <param name="convention">Property naming convention for YAML keys; defaults to PascalCase.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterConfigManagerService(string configName, string configDirectory, YamlNamingConventionType convention = YamlNamingConventionType.PascalCase)
        {
            var config = SquidStdConfig.Load(configName, configDirectory, convention);
            container.RegisterInstance(config, IfAlreadyRegistered.Replace);

            var service = new ConfigManagerService(config, container);
            container.RegisterInstance<IConfigManagerService>(service, IfAlreadyRegistered.Replace);
            container.RegisterInstance(service, IfAlreadyRegistered.Replace);
            container.AddToRegisterTypedList(
                new ServiceRegistrationData(typeof(IConfigManagerService), typeof(ConfigManagerService), -1000)
            );

            return container;
        }

        /// <summary>
        /// Registers only the configuration core: the shared <see cref="DirectoriesConfig" />, the
        /// logger config section and the config manager. Every other service is opt-in via
        /// <c>RegisterCoreServices()</c> or the individual registration methods. Loads a standalone
        /// <see cref="SquidStdConfig" /> for the given name/directory.
        /// </summary>
        /// <remarks>
        /// Registers a fresh SquidStdConfig: do not call this on a container that already has one (the previous instance and its tracked sections would be replaced).
        /// </remarks>
        /// <param name="configName">The logical config name or YAML file name.</param>
        /// <param name="configDirectory">The directory where the config file is searched.</param>
        /// <param name="convention">Property naming convention for YAML keys; defaults to PascalCase.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterConfigServices(string configName, string configDirectory, YamlNamingConventionType convention = YamlNamingConventionType.PascalCase)
            => container.RegisterConfigServices(SquidStdConfig.Load(configName, configDirectory, convention));

        /// <summary>
        /// Registers only the configuration core against an already-loaded <see cref="SquidStdConfig" />:
        /// the config instance itself, the shared <see cref="DirectoriesConfig" />, the logger config
        /// section and the config manager. Every other service is opt-in via <c>RegisterCoreServices()</c>
        /// or the individual registration methods. Once the config instance is registered, every
        /// <c>RegisterConfigSection</c> call on this container binds eagerly against it.
        /// </summary>
        /// <param name="config">The eagerly-loaded configuration.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterConfigServices(SquidStdConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            container.RegisterInstance(new DirectoriesConfig(config.ConfigDirectory, []), IfAlreadyRegistered.Keep);
            container.RegisterConfigSection("logger", static () => new SquidStdLoggerOptions(), -1000);
            container.RegisterDelegate<IConfigManagerService>(
                resolver => new ConfigManagerService(resolver.Resolve<SquidStdConfig>(), (IContainer)resolver),
                Reuse.Singleton
            );

            return container;
        }

        /// <summary>
        /// Registers the default SquidStd services: JSON serializer, event bus, job system,
        /// main-thread dispatcher, timer wheel, metrics collection and secrets. Does NOT register
        /// the configuration core - call this after the bootstrap has been created (which registers
        /// it), or use <c>RegisterCoreServices(string, string)</c> on a standalone container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterCoreServices()
        {
            container.RegisterDataSerializer();
            container.RegisterEventBusService();
            container.RegisterJobSystemService();
            container.RegisterMainThreadDispatcherService();
            container.RegisterTimerWheelService();
            container.RegisterMetricsCollectionService();
            container.RegisterSecretServices();

            return container;
        }

        /// <summary>
        /// Registers the configuration core and the default SquidStd services in one call.
        /// </summary>
        /// <param name="configName">The logical config name or YAML file name.</param>
        /// <param name="configDirectory">The directory where the config file is searched.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterCoreServices(string configName, string configDirectory)
        {
            container.RegisterConfigServices(configName, configDirectory);
            container.RegisterCoreServices();

            return container;
        }

        /// <summary>
        /// Registers the default JSON data serializer for <see cref="IDataSerializer" /> and
        /// <see cref="IDataDeserializer" /> (same singleton instance).
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterDataSerializer()
        {
            var serializer = new JsonDataSerializer();
            container.RegisterInstance<IDataSerializer>(serializer, IfAlreadyRegistered.Keep);
            container.RegisterInstance<IDataDeserializer>(serializer, IfAlreadyRegistered.Keep);

            return container;
        }

        /// <summary>
        /// Registers the default event bus service in the container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterEventBusService()
        {
            container.RegisterInstance(new EventBusOptions());
            container.RegisterDelegate<IEventBus>(
                resolver => new EventBusService(resolver.Resolve<EventBusOptions>()),
                Reuse.Singleton
            );
            container.AddToRegisterTypedList(new ServiceRegistrationData(typeof(IEventBus), typeof(EventBusService), -1));
            container.RegisterStdService<EventListenerActivator, EventListenerActivator>(-900);

            return container;
        }

        /// <summary>
        /// Registers the recursive file watcher service as a singleton resolving the event bus.
        /// Not part of <c>RegisterCoreServices</c>: opt in, then call
        /// <see cref="IFileWatcherService.Watch(string)" /> for the directories to watch.
        /// </summary>
        /// <param name="debounceDelay">Optional debounce window; defaults to 300ms when null.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterFileWatcherService(TimeSpan? debounceDelay = null)
        {
            container.RegisterDelegate<IFileWatcherService>(
                resolver => debounceDelay is { } delay
                                ? new FileWatcherService(resolver.Resolve<IEventBus>(), delay)
                                : new FileWatcherService(resolver.Resolve<IEventBus>()),
                Reuse.Singleton
            );

            return container;
        }

        /// <summary>
        /// Registers the default job system service in the container.
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterJobSystemService(JobsConfig? config = null)
        {
            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("jobs", static () => new JobsConfig(), -100);
            }

            return container.RegisterStdService<IJobSystem, JobSystemService>(-1);
        }

        /// <summary>
        /// Registers the default main-thread dispatcher service in the container.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterMainThreadDispatcherService()
            => container.RegisterStdService<IMainThreadDispatcher, MainThreadDispatcherService>(-1);

        /// <summary>
        /// Registers the default metrics collection service in the container.
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterMetricsCollectionService(MetricsConfig? config = null)
        {
            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("metrics", static () => new MetricsConfig(), -80);
            }

            return container.RegisterStdService<IMetricsCollectionService, MetricsCollectionService>(1000);
        }

        /// <summary>
        /// Registers default encrypted local secret services in the container.
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterSecretServices(SecretsConfig? config = null)
        {
            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("secrets", static () => new SecretsConfig(), -60);
            }

            container.Register<ISecretProtector, AesGcmSecretProtector>(Reuse.Singleton);
            container.Register<ISecretStore, FileSecretStore>(Reuse.Singleton);

            return container;
        }

        /// <summary>
        /// Registers the default timer wheel service in the container.
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterTimerWheelService(TimerWheelConfig? config = null)
        {
            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("timerWheel", static () => new TimerWheelConfig(), -90);
            }

            return container.RegisterStdService<ITimerService, TimerWheelService>(-1);
        }

        /// <summary>
        /// Registers the YAML data serializer for <see cref="IDataSerializer" /> and
        /// <see cref="IDataDeserializer" /> (same singleton instance). Existing registrations
        /// are kept.
        /// </summary>
        /// <param name="convention">Property naming convention for the emitted YAML.</param>
        /// <param name="ignoreUnmatchedProperties">When true (default) unknown YAML keys are ignored.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterYamlDataSerializer(
            YamlNamingConventionType convention = YamlNamingConventionType.PascalCase,
            bool ignoreUnmatchedProperties = true
        )
        {
            var serializer = new YamlDataSerializer(convention, ignoreUnmatchedProperties);
            container.RegisterInstance<IDataSerializer>(serializer, IfAlreadyRegistered.Keep);
            container.RegisterInstance<IDataDeserializer>(serializer, IfAlreadyRegistered.Keep);

            return container;
        }
    }
}
