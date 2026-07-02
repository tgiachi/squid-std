<h1 align="center">SquidStd.Templating</h1>

Templating for SquidStd, backed by [Scriban](https://github.com/scriban/scriban). `ITemplateRenderer`
renders ad-hoc template strings and named templates (compiled and cached), and auto-loads every
`templates/**/*.tmpl` file at startup by relative path without extension (`emails/welcome.tmpl` →
`emails/welcome`). Scriban default member naming applies (`snake_case`): a `UserName` property is
`{{ user.user_name }}`.

## Install

```bash
dotnet add package SquidStd.Templating
```

## Usage

```csharp
using DryIoc;
using SquidStd.Templating.Extensions;
using SquidStd.Templating.Interfaces;

container.AddTemplating(); // after RegisterCoreServices (a shared DirectoriesConfig is registered there)

var renderer = container.Resolve<ITemplateRenderer>();

// ad-hoc
var hi = await renderer.RenderAsync("Hi {{ user.name }}!", new { User = new { Name = "squid" } });

// named (registered manually or auto-loaded from templates/)
renderer.Register("welcome", "Welcome {{ user.name }}");
var welcome = await renderer.RenderByNameAsync("welcome", new { User = new { Name = "squid" } });
```

## Key types

| Type                               | Purpose                                                                                    |
|------------------------------------|--------------------------------------------------------------------------------------------|
| `ITemplateRenderer`                | Render ad-hoc and named templates.                                                         |
| `ScribanTemplateRenderer`          | Scriban implementation; compiles/caches named templates and auto-loads `templates/*.tmpl`. |
| `TemplateException`                | Raised on parse/render failures.                                                           |
| `TemplatingRegistrationExtensions` | `AddTemplating(...)` registration.                                                         |

## Related

- Tutorial: [Templating](https://tgiachi.github.io/squid-std/tutorials/templating.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
