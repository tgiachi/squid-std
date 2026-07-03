namespace SquidStd.ConsoleCommands.Attributes;

/// <summary>
/// Marks an <see cref="Interfaces.IConsoleCommandExecutor" /> class for source-generated registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterConsoleCommandAttribute : Attribute
{
    /// <summary>Primary command name or aliases separated by <c>|</c>.</summary>
    public string CommandName { get; }

    /// <summary>Help description.</summary>
    public string Description { get; }

    /// <summary>Initializes the attribute.</summary>
    /// <param name="commandName">Primary command name or aliases separated by <c>|</c>.</param>
    /// <param name="description">Help description.</param>
    public RegisterConsoleCommandAttribute(string commandName, string description = "")
    {
        CommandName = commandName;
        Description = description;
    }
}
