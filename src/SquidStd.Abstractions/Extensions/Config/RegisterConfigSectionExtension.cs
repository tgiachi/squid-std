using DryIoc;
using Serilog;
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
            if (container.IsRegistered<SquidStdConfig>())
            {
                var trackedUnderOtherSection = container.Resolve<SquidStdConfig>()
                                                         .Entries
                                                         .Any(
                                                             entry => entry.ConfigType == configType &&
                                                                      !string.Equals(
                                                                          entry.SectionName,
                                                                          sectionName,
                                                                          StringComparison.Ordinal
                                                                      )
                                                         );

                if (trackedUnderOtherSection)
                {
                    throw new InvalidOperationException($"Config type '{configType.Name}' is already registered.");
                }
            }

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

        // Degenerate mode (bare container without the bootstrap/SquidStdConfig): register the
        // default instance; there is no file to bind from.
        var fallback = createDefault?.Invoke() ?? new TConfig();
        container.RegisterInstance(fallback, IfAlreadyRegistered.Keep);

        return container;
    }
}
