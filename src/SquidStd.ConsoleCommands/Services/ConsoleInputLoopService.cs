using System.Text;
using Serilog;
using Serilog.Events;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.ConsoleCommands.Data.Config;
using SquidStd.ConsoleCommands.Interfaces;

namespace SquidStd.ConsoleCommands.Services;

/// <summary>
/// Reads keystrokes from the interactive terminal and drives the prompt UI: printable input,
/// backspace, TAB autocomplete cycling, command history, ESC clearing and Enter dispatching to
/// the command system. Does nothing when the console is not interactive.
/// </summary>
public sealed class ConsoleInputLoopService : ISquidStdService, IDisposable
{
    private readonly List<string> _autocompleteCandidates = [];
    private readonly List<string> _commandHistory = [];
    private readonly ICommandSystemService _commands;
    private readonly ConsoleCommandsConfig _config;
    private readonly CancellationTokenSource _lifetimeCts = new();
    private readonly ILogger _logger = Log.ForContext<ConsoleInputLoopService>();
    private readonly IConsoleUiService _ui;

    private int _autocompleteIndex = -1;
    private string _autocompleteSeed = string.Empty;
    private int _commandHistoryIndex = -1;
    private Thread? _inputThread;

    /// <summary>Initializes the console input loop.</summary>
    /// <param name="ui">Prompt UI that renders input and log lines.</param>
    /// <param name="commands">Command system that executes entered lines.</param>
    /// <param name="config">Prompt configuration section.</param>
    public ConsoleInputLoopService(
        IConsoleUiService ui,
        ICommandSystemService commands,
        ConsoleCommandsConfig config
    )
    {
        _ui = ui;
        _commands = commands;
        _config = config;
    }

    /// <summary>
    /// Starts the background input thread when the console is interactive; otherwise the loop
    /// stays disabled and this call is a no-op.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A completed task.</returns>
    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (!_ui.IsInteractive)
        {
            _logger.Debug("Console is not interactive; input loop disabled");

            return ValueTask.CompletedTask;
        }

        _logger.Information("Interactive console prompt enabled.");

        try
        {
            if (_config.StartLocked)
            {
                _ui.LockInput();
                _ui.WriteLogLine(
                    $"Console input is locked. Press '{_config.UnlockCharacter}' to unlock.",
                    LogEventLevel.Warning
                );
            }

            _inputThread = new Thread(InputLoop)
            {
                IsBackground = true,
                Name = "SquidStd-ConsoleInput"
            };
            _inputThread.Start();
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or ArgumentOutOfRangeException)
        {
            _logger.Warning(
                ex,
                "Interactive console prompt disabled because the current terminal does not support prompt rendering."
            );
            _inputThread = null;
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Stops the input loop by cancelling it and joining the input thread.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A completed task.</returns>
    /// <remarks>
    /// The loop polls <see cref="Console.KeyAvailable" /> and only calls
    /// <see cref="Console.ReadKey(bool)" /> when a key is buffered, so it normally reacts to
    /// cancellation within one poll interval. The join uses a 2 second timeout: if the thread is
    /// ever stuck in a blocked read it is a background thread and ends with the process.
    /// </remarks>
    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        _lifetimeCts.Cancel();
        _inputThread?.Join(TimeSpan.FromSeconds(2));
        _inputThread = null;

        return ValueTask.CompletedTask;
    }

    private void ApplyAutocomplete(StringBuilder buffer, bool reverse)
    {
        var currentInput = buffer.ToString();

        if (_autocompleteSeed.Length == 0 || !string.Equals(_autocompleteSeed, currentInput, StringComparison.Ordinal))
        {
            _autocompleteSeed = currentInput;
            _autocompleteCandidates.Clear();
            _autocompleteCandidates.AddRange(_commands.GetAutocompleteSuggestions(currentInput));
            _autocompleteIndex = -1;
        }

        if (_autocompleteCandidates.Count == 0)
        {
            return;
        }

        _autocompleteIndex = reverse
                                 ? _autocompleteIndex <= 0 ? _autocompleteCandidates.Count - 1 : _autocompleteIndex - 1
                                 : (_autocompleteIndex + 1) % _autocompleteCandidates.Count;

        var suggestion = _autocompleteCandidates[_autocompleteIndex];
        buffer.Clear();
        buffer.Append(suggestion);
        _ui.UpdateInput(buffer.ToString());
    }

    private void InputLoop()
    {
        var cancellationToken = _lifetimeCts.Token;
        var buffer = new StringBuilder();
        var lockWarningShown = false;

        try
        {
            _ui.UpdateInput(string.Empty);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!Console.KeyAvailable)
                {
                    if (cancellationToken.WaitHandle.WaitOne(25))
                    {
                        break;
                    }

                    continue;
                }

                var key = Console.ReadKey(true);

                if (_ui.IsInputLocked)
                {
                    if (key.KeyChar == _config.UnlockCharacter)
                    {
                        _ui.UnlockInput();
                        lockWarningShown = false;
                        _ui.WriteLogLine("Console unlocked.", LogEventLevel.Information);
                    }
                    else if (!lockWarningShown)
                    {
                        _ui.WriteLogLine(
                            $"Console input is locked. Press '{_config.UnlockCharacter}' to unlock.",
                            LogEventLevel.Warning
                        );
                        lockWarningShown = true;
                    }

                    continue;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    SubmitCommand(buffer.ToString(), cancellationToken);
                    buffer.Clear();
                    _commandHistoryIndex = -1;
                    ResetAutocompleteState();
                    _ui.UpdateInput(string.Empty);
                    lockWarningShown = false;

                    continue;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                    {
                        buffer.Length--;
                        ResetAutocompleteState();
                        _ui.UpdateInput(buffer.ToString());
                    }

                    lockWarningShown = false;

                    continue;
                }

                if (key.Key == ConsoleKey.Escape)
                {
                    buffer.Clear();
                    _commandHistoryIndex = -1;
                    ResetAutocompleteState();

                    if (_config.StartLocked)
                    {
                        _ui.LockInput();
                    }

                    _ui.UpdateInput(string.Empty);
                    lockWarningShown = false;

                    continue;
                }

                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (_commandHistory.Count == 0)
                    {
                        continue;
                    }

                    if (_commandHistoryIndex < _commandHistory.Count - 1)
                    {
                        _commandHistoryIndex++;
                    }

                    buffer.Clear();
                    buffer.Append(_commandHistory[_commandHistory.Count - 1 - _commandHistoryIndex]);
                    ResetAutocompleteState();
                    _ui.UpdateInput(buffer.ToString());
                    lockWarningShown = false;

                    continue;
                }

                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (_commandHistory.Count == 0)
                    {
                        continue;
                    }

                    if (_commandHistoryIndex > 0)
                    {
                        _commandHistoryIndex--;
                        buffer.Clear();
                        buffer.Append(_commandHistory[_commandHistory.Count - 1 - _commandHistoryIndex]);
                    }
                    else
                    {
                        _commandHistoryIndex = -1;
                        buffer.Clear();
                    }

                    ResetAutocompleteState();
                    _ui.UpdateInput(buffer.ToString());
                    lockWarningShown = false;

                    continue;
                }

                if (key.Key == ConsoleKey.Tab)
                {
                    ApplyAutocomplete(buffer, (key.Modifiers & ConsoleModifiers.Shift) != 0);
                    lockWarningShown = false;

                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                {
                    buffer.Append(key.KeyChar);
                    ResetAutocompleteState();
                    _ui.UpdateInput(buffer.ToString());
                    lockWarningShown = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or ArgumentOutOfRangeException)
        {
            _logger.Warning(ex, "Console input loop stopped: the terminal no longer supports interactive input.");
        }
    }

    private void ResetAutocompleteState()
    {
        _autocompleteSeed = string.Empty;
        _autocompleteIndex = -1;
        _autocompleteCandidates.Clear();
    }

    private void SubmitCommand(string rawCommand, CancellationToken cancellationToken)
    {
        var command = rawCommand.Trim();

        if (command.Length == 0)
        {
            return;
        }

        _commandHistory.Add(command);

        _logger.Verbose("Console command entered: {Command}", command);

        _commands.ExecuteCommandAsync(command, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Cancels the input loop and releases the lifetime token source.</summary>
    public void Dispose()
    {
        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
    }
}
