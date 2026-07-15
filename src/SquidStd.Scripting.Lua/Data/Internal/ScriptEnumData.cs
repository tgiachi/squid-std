namespace SquidStd.Scripting.Lua.Data.Internal;

/// <summary>
/// Record describing an enum type to expose to Lua as a read-only global table, keyed by member
/// name. Registered through <c>RegisterScriptEnum</c> and consumed during engine initialization.
/// </summary>
public sealed record ScriptEnumData
{
    /// <summary>
    /// The .NET enum type to expose to Lua.
    /// </summary>
    public Type EnumType { get; }

    /// <summary>
    /// Initializes a new instance of the ScriptEnumData record.
    /// </summary>
    /// <param name="enumType">The .NET enum type to expose to Lua.</param>
    /// <exception cref="ArgumentNullException">Thrown when enumType is null.</exception>
    /// <exception cref="ArgumentException">Thrown when enumType is not an enum.</exception>
    public ScriptEnumData(Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
        {
            throw new ArgumentException("Type must be an enum.", nameof(enumType));
        }

        EnumType = enumType;
    }
}
