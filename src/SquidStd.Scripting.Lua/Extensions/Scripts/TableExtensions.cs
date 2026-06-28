using System.Reflection;
using MoonSharp.Interpreter;
using SquidStd.Scripting.Lua.Proxies;

namespace SquidStd.Scripting.Lua.Extensions.Scripts;

/// <summary>
///     Provides extension methods for MoonSharp Table objects to enable proxying to interfaces.
/// </summary>
public static class TableExtensions
{
    extension(Table table)
    {
        /// <summary>
        ///     Converts a MoonSharp Table to a proxy implementing the specified interface.
        /// </summary>
        public TInterface ToProxy<TInterface>()
            where TInterface : class
        {
            var proxy = DispatchProxy.Create<TInterface, LuaProxy<TInterface>>();
            ((LuaProxy<TInterface>)(object)proxy).Table = table;

            return proxy;
        }
    }
}
