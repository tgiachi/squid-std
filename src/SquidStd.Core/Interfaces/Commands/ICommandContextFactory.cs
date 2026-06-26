namespace SquidStd.Core.Interfaces.Commands;

/// <summary>
///     Produces the current <typeparamref name="TContext" /> for a context-less dispatch
///     (for example resolving the current session).
/// </summary>
/// <typeparam name="TContext">The context type produced.</typeparam>
public interface ICommandContextFactory<out TContext>
{
    /// <summary>Creates the current context.</summary>
    /// <returns>The context to pass to handlers.</returns>
    TContext Create();
}
