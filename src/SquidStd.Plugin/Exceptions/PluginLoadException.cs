namespace SquidStd.Plugin.Exceptions;

/// <summary>
/// Raised when the plugin loader cannot discover, order, or instantiate plugins.
/// </summary>
public class PluginLoadException : Exception
{
    /// <summary>Initializes the exception with a message describing the failure.</summary>
    public PluginLoadException(string message)
        : base(message) { }

    /// <summary>Initializes the exception with a message and the underlying cause.</summary>
    public PluginLoadException(string message, Exception innerException)
        : base(message, innerException) { }
}
