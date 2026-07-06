using DryIoc;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Scripting.Lua.Data.Config;
using SquidStd.Scripting.Lua.Interfaces.Scripts;
using SquidStd.Scripting.Lua.Services;

namespace SquidStd.Scripting.Lua.Extensions.Scripts;

/// <summary>
/// Registration helpers for the Lua script engine.
/// </summary>
public static class RegisterLuaEngineExtension
{
    /// <param name="container">Container that receives the registrations.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the Lua script engine with its explicit configuration: the config instance
        /// and the <see cref="LuaScriptEngineService" /> as the standard script engine service.
        /// Replaces the manual RegisterInstance + RegisterStdService pair.
        /// </summary>
        /// <param name="config">The Lua engine configuration.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterLuaEngine(LuaEngineConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            container.RegisterInstance(config, IfAlreadyRegistered.Replace);

            return container.RegisterStdService<IScriptEngineService, LuaScriptEngineService>();
        }
    }
}
