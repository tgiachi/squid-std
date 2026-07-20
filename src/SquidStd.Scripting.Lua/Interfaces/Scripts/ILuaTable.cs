namespace SquidStd.Scripting.Lua.Interfaces.Scripts;

/// <summary>
/// Marks a type that projects itself into a Lua table via an explicit <see cref="ToDictionary" /> (no
/// reflection). When a Lua-facing function returns such a type, the engine marshals it by calling
/// <see cref="ToDictionary" /> instead of exposing the object as userdata — so the type owns the exact
/// Lua-facing shape (key names, value flattening) and never leaks its live members to scripts.
/// </summary>
public interface ILuaTable
{
    /// <summary>Returns the field map exposed to Lua as a table.</summary>
    Dictionary<string, object?> ToDictionary();
}
