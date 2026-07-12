using SquidStd.Core.Types.Yaml;

namespace SquidStd.Core.Data.Bootstrap;

/// <summary>
/// Defines bootstrap-only options used to locate SquidStd runtime resources.
/// </summary>
public sealed class SquidStdOptions
{
    /// <summary>
    /// Gets or sets the root directory for configuration, logs, and runtime data.
    /// </summary>
    public string RootDirectory { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>
    /// Gets or sets the logical configuration name or YAML file name.
    /// </summary>
    public string ConfigName { get; set; } = "squidstd";

    /// <summary>
    /// Gets or sets the application name used in the startup banner and as the Serilog
    /// "Application" property. When null or empty, the entry assembly name is used.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// Gets or sets the application version used in the startup banner and as the Serilog
    /// "ApplicationVersion" property. When null or empty, the entry assembly informational
    /// version is used.
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// Explicit logger configuration. When set, the "logger" YAML section is not bound and
    /// Serilog is configured from this instance only; the file cannot override it.
    /// </summary>
    public SquidStdLoggerOptions? Logger { get; set; }

    /// <summary>
    /// Directory names created under <see cref="RootDirectory" /> when the bootstrap is created
    /// (snake_case on disk, same rule as directory path resolution).
    /// </summary>
    public string[] Directories { get; set; } = [];

    /// <summary>
    /// Naming convention for YAML property keys in the configuration file (PascalCase by
    /// default). Section names are always matched exactly as registered.
    /// </summary>
    public YamlNamingConventionType YamlNamingConvention { get; set; } = YamlNamingConventionType.PascalCase;
}
