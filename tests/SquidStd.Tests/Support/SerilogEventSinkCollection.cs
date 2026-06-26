namespace SquidStd.Tests.Support;

/// <summary>
///     Serializes the tests that mutate the static <c>EventSink.OnLogReceived</c> handler,
///     preventing cross-talk between parallel test classes.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class SerilogEventSinkCollection
{
    public const string Name = "SerilogEventSink";
}
