namespace SquidStd.Actors.Interfaces;

/// <summary>
/// Non-generic request facet allowing the actor infrastructure to fault a pending request
/// without knowing its reply type.
/// </summary>
public interface IActorRequestCore
{
    /// <summary>Faults the request's completion with the given error.</summary>
    /// <param name="error">The failure to surface to the caller.</param>
    void Fail(Exception error);
}
