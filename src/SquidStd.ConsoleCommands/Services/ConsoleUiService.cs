using Serilog.Events;
using Spectre.Console;
using SquidStd.ConsoleCommands.Data.Config;
using SquidStd.ConsoleCommands.Interfaces;

namespace SquidStd.ConsoleCommands.Services;

/// <summary>
/// Console UI for the command prompt. On an interactive terminal the prompt is pinned to the
/// bottom row and log/output lines are written above it under a shared lock. In non-interactive
/// environments (redirected streams) every write is a plain
/// <see cref="Console.WriteLine(string)" /> and input state is inert.
/// </summary>
public sealed class ConsoleUiService : IConsoleUiService
{
    private readonly string _lockedPromptPrefix;
    private readonly string _promptPrefix;
    private readonly Lock _sync = new();

    private string _input = string.Empty;

    /// <inheritdoc />
    public bool IsInteractive { get; private set; }

    /// <inheritdoc />
    public bool IsInputLocked { get; private set; }

    /// <summary>Initializes the console UI from the prompt configuration.</summary>
    /// <param name="config">Prompt configuration section.</param>
    public ConsoleUiService(ConsoleCommandsConfig config)
    {
        _promptPrefix = config.Prompt;
        _lockedPromptPrefix = BuildLockedPromptPrefix(config.Prompt);
        IsInteractive = IsInteractiveConsole();
        IsInputLocked = config.StartLocked;
    }

    /// <inheritdoc />
    public void LockInput()
    {
        if (!IsInteractive)
        {
            IsInputLocked = true;

            return;
        }

        lock (_sync)
        {
            IsInputLocked = true;
            _input = string.Empty;
            RenderPromptUnsafe();
        }
    }

    /// <inheritdoc />
    public void UnlockInput()
    {
        if (!IsInteractive)
        {
            IsInputLocked = false;

            return;
        }

        lock (_sync)
        {
            IsInputLocked = false;
            RenderPromptUnsafe();
        }
    }

    /// <inheritdoc />
    public void UpdateInput(string input)
    {
        if (!IsInteractive)
        {
            return;
        }

        lock (_sync)
        {
            _input = input;
            RenderPromptUnsafe();
        }
    }

    /// <inheritdoc />
    public void WriteLine(string line)
    {
        if (!IsInteractive)
        {
            Console.WriteLine(line);

            return;
        }

        lock (_sync)
        {
            try
            {
                ClearPromptRowUnsafe();

                foreach (var item in SplitLines(line))
                {
                    Console.WriteLine(item);
                }

                RenderPromptUnsafe();
            }
            catch (IOException)
            {
                IsInteractive = false;
                Console.WriteLine(line);
            }
            catch (ArgumentOutOfRangeException)
            {
                IsInteractive = false;
                Console.WriteLine(line);
            }
        }
    }

    /// <inheritdoc />
    public void WriteLogLine(string line, LogEventLevel level)
    {
        if (!IsInteractive)
        {
            Console.WriteLine(line);

            return;
        }

        lock (_sync)
        {
            try
            {
                ClearPromptRowUnsafe();

                foreach (var item in SplitLines(line))
                {
                    AnsiConsole.MarkupLine(FormatLogMarkup(item, level));
                }

                RenderPromptUnsafe();
            }
            catch (IOException)
            {
                IsInteractive = false;
                Console.WriteLine(line);
            }
            catch (ArgumentOutOfRangeException)
            {
                IsInteractive = false;
                Console.WriteLine(line);
            }
        }
    }

    private static string BuildLockedPromptPrefix(string prompt)
    {
        var name = prompt.TrimEnd().TrimEnd('>').TrimEnd();

        if (name.Length == 0)
        {
            return "[LOCKED]> ";
        }

        return name + " [LOCKED]> ";
    }

    private void ClearPromptRowUnsafe()
    {
        var width = Math.Max(1, Console.WindowWidth);
        var promptRow = GetPromptRowUnsafe();

        Console.SetCursorPosition(0, promptRow);
        Console.Write(new string(' ', width));
        Console.SetCursorPosition(0, promptRow);
    }

    private static string FormatLogMarkup(string line, LogEventLevel level)
    {
        var escaped = Markup.Escape(line);

        return level switch
        {
            LogEventLevel.Verbose => $"[grey]{escaped}[/]",
            LogEventLevel.Debug => $"[grey]{escaped}[/]",
            LogEventLevel.Warning => $"[yellow]{escaped}[/]",
            LogEventLevel.Error => $"[red]{escaped}[/]",
            LogEventLevel.Fatal => $"[red]{escaped}[/]",
            _ => escaped
        };
    }

    private static int GetPromptRowUnsafe()
    {
        var bufferHeight = Math.Max(1, Console.BufferHeight);
        var windowHeight = Math.Max(1, Console.WindowHeight);
        var row = Console.WindowTop + windowHeight - 1;

        return Math.Clamp(row, 0, bufferHeight - 1);
    }

    private static bool IsInteractiveConsole()
    {
        if (!Environment.UserInteractive)
        {
            return false;
        }

        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            return false;
        }

        return true;
    }

    private void RenderPromptUnsafe()
    {
        var width = Math.Max(1, Console.WindowWidth);
        var promptRow = GetPromptRowUnsafe();
        var promptPrefix = IsInputLocked ? _lockedPromptPrefix : _promptPrefix;

        Console.SetCursorPosition(0, promptRow);
        Console.Write(new string(' ', width));
        Console.SetCursorPosition(0, promptRow);

        var line = promptPrefix + _input;
        var visible = line.Length > width ? line[..width] : line;
        AnsiConsole.Console.Write(new Text(visible, new(Color.Green)));

        var cursorColumn = Math.Min(width - 1, promptPrefix.Length + _input.Length);
        Console.SetCursorPosition(cursorColumn, promptRow);
    }

    private static IEnumerable<string> SplitLines(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            yield return string.Empty;

            yield break;
        }

        var split = line.Replace("\r", string.Empty, StringComparison.Ordinal)
                        .Split('\n');

        var length = split.Length;

        if (length > 0 && split[length - 1].Length == 0)
        {
            length--;
        }

        for (var i = 0; i < length; i++)
        {
            yield return split[i];
        }
    }
}
