using Serilog.Core;
using Serilog.Events;
using SquidStd.ConsoleCommands.Interfaces;

namespace SquidStd.ConsoleCommands.Internal.Logging;

/// <summary>
/// Serilog sink that routes formatted log lines through the console UI, keeping the prompt intact.
/// </summary>
internal sealed class ConsolePromptLogSink : ILogEventSink
{
    private readonly IConsoleUiService _ui;

    public ConsolePromptLogSink(IConsoleUiService ui)
    {
        _ui = ui;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        var line = $"[{logEvent.Timestamp:HH:mm:ss} {ShortLevel(logEvent.Level)}] {message}";

        if (logEvent.Exception is not null)
        {
            line += Environment.NewLine + logEvent.Exception;
        }

        _ui.WriteLogLine(line, logEvent.Level);
    }

    private static string ShortLevel(LogEventLevel level)
        => level switch
        {
            LogEventLevel.Verbose => "VRB",
            LogEventLevel.Debug => "DBG",
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            _ => "FTL"
        };
}
