<h1 align="center">SquidStd.Plugin</h1>

Loader for SquidStd plugins. Wires internal and external `ISquidStdPlugin` implementations into the
bootstrap: it collects them, resolves the dependency order across the whole set, and calls `Configure`
on each one before the bootstrap starts.

## Install

```bash
dotnet add package SquidStd.Plugin
```

## Usage

```csharp
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Plugin.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(new SquidStdOptions
{
    ConfigName = "myapp",
    RootDirectory = "./data"
});

bootstrap.UsePlugins(plugins =>
{
    plugins.Add<WebPlugin>();          // internal, by type
    plugins.Add(new MetricsPlugin());  // internal, by instance
    plugins.FromDirectory("plugins");  // external assemblies (*.dll)
});

await bootstrap.RunAsync();
```

## How loading works

- Plugins are ordered by dependency: `PluginMetadata.Dependencies` (plugin ids, compared
  case-insensitively) is resolved across internal and external plugins together, so an external plugin
  can depend on an internal one and vice versa.
- `Configure` runs before the bootstrap starts, so plugins can register their own configuration sections and services against the container.
- Plugin directories are managed like the other bootstrap directories: a missing directory is
  created on the spot and simply yields no plugins.
- Each plugin receives a `PluginContext` populated with the standard keys: `PluginContextKeys.RootDirectory`
  (the bootstrap root directory) and `PluginContextKeys.AppName` (the bootstrap app name).

## Trusted plugins

External assemblies load into the default `AssemblyLoadContext`: there is no unloading and no
per-plugin version isolation, so plugins are expected to be trusted. A failing plugin stops startup,
either with a `PluginLoadException` (discovery, ordering, or instantiation failure) or with the
plugin's own exception from `Configure`.

Ordering with `ConfigureLogging()` is free since the config-first bootstrap: configuration sections
bind eagerly at registration, so plugins can register their sections at any point before the services
consume them. Calling `ConfigureLogging()` before `UsePlugins` makes the plugin-load log lines visible.
The one residual rule: `OnConfigLoaded` hooks targeting a plugin's section must be registered before
`ConfigureLogging()` runs, because hooks are applied there.

## Related

- Contracts: [SquidStd.Plugin.Abstractions](https://tgiachi.github.io/squid-std/articles/plugin-abstractions.html)
- [SquidStd.Core](https://tgiachi.github.io/squid-std/articles/core.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
