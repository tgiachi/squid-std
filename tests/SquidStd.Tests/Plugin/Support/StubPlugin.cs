using DryIoc;
using SquidStd.Plugin.Abstractions.Data;
using SquidStd.Plugin.Abstractions.Interfaces.Plugins;

namespace SquidStd.Tests.Plugin.Support;

internal sealed class StubPlugin : ISquidStdPlugin
{
    public PluginMetadata Metadata { get; }

    public StubPlugin(string id, params string[] dependencies)
    {
        Metadata = new()
        {
            Id = id,
            Name = id,
            Version = new(1, 0, 0),
            Author = "Tests",
            Dependencies = dependencies
        };
    }

    public void Configure(IContainer container, PluginContext context) { }
}
