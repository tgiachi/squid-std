namespace SquidStd.Abstractions.Data.Internal.Services;

public record ServiceRegistrationData(Type ServiceType, Type ImplementationType, int Priority = 0);
