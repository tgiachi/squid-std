namespace SquidStd.Scripting.Lua.Attributes;

/// <summary>
/// Marks a Lua script module for generated registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RegisterScriptModuleAttribute : Attribute { }
