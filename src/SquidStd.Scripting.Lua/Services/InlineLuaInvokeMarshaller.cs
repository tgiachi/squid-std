using MoonSharp.Interpreter;
using SquidStd.Scripting.Lua.Interfaces.Events;

namespace SquidStd.Scripting.Lua.Services;

/// <summary>Default marshaller: runs the callback inline on the calling thread.</summary>
public sealed class InlineLuaInvokeMarshaller : ILuaInvokeMarshaller
{
    public DynValue Invoke(Func<DynValue> call)
    {
        ArgumentNullException.ThrowIfNull(call);

        return call();
    }
}
