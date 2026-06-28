<h1 align="center">SquidStd.Plugin.Abstractions</h1>

Contracts for building SquidStd plugins. A plugin declares its identity through `PluginMetadata` and
registers its services into the host's DryIoc container via `Configure`, receiving a `PluginContext`
with shared boot data.

## Install

```bash
dotnet add package SquidStd.Plugin.Abstractions
```

## Usage

```csharp
using DryIoc;
using SquidStd.Plugin.Abstractions.Data;
using SquidStd.Plugin.Abstractions.Interfaces.Plugins;

public sealed class MyPlugin : ISquidStdPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "com.example.myplugin",
        Name = "My Plugin",
        Version = new Version(1, 0, 0),
        Author = "me"
    };

    public void Configure(IContainer container, PluginContext context)
    {
        // register the plugin's services / config sections here
    }
}
```

## Key types

| Type              | Purpose                                       |
|-------------------|-----------------------------------------------|
| `ISquidStdPlugin` | Plugin entry point (`Metadata`, `Configure`). |
| `PluginMetadata`  | Plugin identity and dependency declarations.  |
| `PluginContext`   | Shared boot data passed to the plugin.        |

## Related

- Tutorial: [Plugins](https://tgiachi.github.io/squid-std/tutorials/plugins.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
