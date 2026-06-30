namespace SquidStd.Workers.Attributes;

/// <summary>
/// Marks a worker job handler for generated registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RegisterJobHandlerAttribute : Attribute { }
