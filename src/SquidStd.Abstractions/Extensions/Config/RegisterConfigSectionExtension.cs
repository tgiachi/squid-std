using DryIoc;
using Serilog;
using SquidStd.Abstractions.Data.Internal.Config;
using SquidStd.Abstractions.Extensions.Container;
using SquidStd.Core.Config;

namespace SquidStd.Abstractions.Extensions.Config;

public static class RegisterConfigSectionExtension
{
    public static IContainer RegisterConfigSection<TConfig>(
        this IContainer container,
        string sectionName,
        Func<TConfig>? createDefault = null,
        int priority = 0
    )
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        var configType = typeof(TConfig);

        if (container.IsRegistered<TConfig>())
        {
            Log.ForContext(typeof(RegisterConfigSectionExtension))
               .Debug(
                   "Config section {Section:l} skipped: an explicit {ConfigType:l} instance is already registered",
                   sectionName,
                   configType.Name
               );

            return container;
        }

        if (container.IsRegistered<SquidStdConfig>())
        {
            var squidConfig = container.Resolve<SquidStdConfig>();
            var bound = squidConfig.BindSection(sectionName, createDefault, priority);
            container.RegisterInstance(bound, IfAlreadyRegistered.Replace);

            return container;
        }

        // legacy deferred path below, unchanged (removed by the cleanup task)

        if (container.IsRegistered<List<ConfigRegistrationData>>())
        {
            var entries = container.Resolve<List<ConfigRegistrationData>>();
            var sameSection = entries.FirstOrDefault(
                entry => string.Equals(
                    entry.SectionName,
                    sectionName,
                    StringComparison.Ordinal
                )
            );

            if (sameSection is not null)
            {
                if (sameSection.ConfigType == configType)
                {
                    return container;
                }

                throw new InvalidOperationException($"Config section '{sectionName}' is already registered.");
            }

            if (entries.Any(entry => entry.ConfigType == configType))
            {
                throw new InvalidOperationException($"Config type '{configType.Name}' is already registered.");
            }
        }

        var factory = createDefault ?? (() => new());
        container.AddToRegisterTypedList(new ConfigRegistrationData(sectionName, configType, () => factory(), priority));

        return container;
    }
}
