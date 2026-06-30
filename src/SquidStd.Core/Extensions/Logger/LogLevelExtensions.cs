using Serilog.Events;
using SquidStd.Core.Types;

namespace SquidStd.Core.Extensions.Logger;

/// <summary>
/// Extension methods for converting log levels between different logging frameworks.
/// </summary>
public static class LogLevelExtensions
{
    /// <param name="logLevel">The log level to convert.</param>
    extension(LogLevelType logLevel)
    {
        /// <summary>
        /// Converts a LogLevelType to a Serilog LogEventLevel.
        /// </summary>
        /// <returns>The corresponding Serilog log event level.</returns>
        public LogEventLevel ToSerilogLogLevel()
            => logLevel switch
            {
                LogLevelType.Trace       => LogEventLevel.Verbose,
                LogLevelType.Debug       => LogEventLevel.Debug,
                LogLevelType.Information => LogEventLevel.Information,
                LogLevelType.Warning     => LogEventLevel.Warning,
                LogLevelType.Error       => LogEventLevel.Error,
                LogLevelType.Critical    => LogEventLevel.Fatal,
                _                        => LogEventLevel.Information
            };
    }
}
