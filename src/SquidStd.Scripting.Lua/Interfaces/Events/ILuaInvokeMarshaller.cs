using MoonSharp.Interpreter;

namespace SquidStd.Scripting.Lua.Interfaces.Events;

/// <summary>
/// Marshals a Lua callback invocation onto the correct thread. The default runs the call inline;
/// hosts can supply an implementation that marshals onto a game-loop / single-writer thread.
/// </summary>
public interface ILuaInvokeMarshaller
{
    /// <summary>
    /// Runs <paramref name="call" /> (which builds the payload table and invokes the Lua callback),
    /// returning its result when executed inline, or a nil value when marshalled to run later.
    /// </summary>
    DynValue Invoke(Func<DynValue> call);
}
