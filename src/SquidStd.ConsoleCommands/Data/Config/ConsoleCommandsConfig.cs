namespace SquidStd.ConsoleCommands.Data.Config;

/// <summary>
/// Configuration for the interactive console command prompt.
/// </summary>
public sealed class ConsoleCommandsConfig
{
    /// <summary>Prompt prefix shown on the input row.</summary>
    public string Prompt { get; set; } = "squid> ";

    /// <summary>Whether the prompt starts locked.</summary>
    public bool StartLocked { get; set; } = true;

    /// <summary>Character that unlocks the prompt when locked.</summary>
    public char UnlockCharacter { get; set; } = '*';
}
