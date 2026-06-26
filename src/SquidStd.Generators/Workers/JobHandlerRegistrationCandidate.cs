using Microsoft.CodeAnalysis;

namespace SquidStd.Generators.Workers;

internal sealed class JobHandlerRegistrationCandidate
{
    public JobHandlerRegistrationCandidate(
        string handlerTypeName,
        string displayName,
        Location? location,
        bool isSupported
    )
    {
        HandlerTypeName = handlerTypeName;
        DisplayName = displayName;
        Location = location;
        IsSupported = isSupported;
    }

    public string HandlerTypeName { get; }

    public string DisplayName { get; }

    public Location? Location { get; }

    public bool IsSupported { get; }
}
