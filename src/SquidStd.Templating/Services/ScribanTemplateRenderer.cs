using System.Collections.Concurrent;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Core.Directories;
using SquidStd.Templating.Interfaces;
using SquidStd.Templating.Internal;

namespace SquidStd.Templating.Services;

/// <summary>
/// Scriban-backed <see cref="ITemplateRenderer" />. Named templates are compiled and cached; ad-hoc
/// strings are parsed per call. On start it auto-loads <c>templates/**/*.tmpl</c>.
/// </summary>
public sealed class ScribanTemplateRenderer : ITemplateRenderer, ISquidStdService
{
    private const string TemplateExtension = ".tmpl";

    private readonly DirectoriesConfig _directories;
    private readonly ConcurrentDictionary<string, Template> _named = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _sources = new(StringComparer.Ordinal);
    private readonly NamedTemplateLoader _loader;

    public ScribanTemplateRenderer(DirectoriesConfig directories)
    {
        _directories = directories;
        _loader = new NamedTemplateLoader(_sources);
    }

    /// <inheritdoc />
    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        var directory = _directories["templates"];

        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*" + TemplateExtension, SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(directory, file).Replace('\\', '/');
            var name = relative[..^TemplateExtension.Length];
            var content = await File.ReadAllTextAsync(file, cancellationToken);

            Register(name, content);
        }
    }

    /// <inheritdoc />
    public ValueTask StopAsync(CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public void Register(string name, string template)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        _named[name] = Parse(template, name);
        _sources[name] = template;
    }

    /// <inheritdoc />
    public async ValueTask<string> RenderAsync(string template, object? model, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        return await RenderCompiledAsync(Parse(template, "(inline)"), model, "(inline)");
    }

    /// <inheritdoc />
    public async ValueTask<string> RenderByNameAsync(
        string name,
        object? model,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_named.TryGetValue(name, out var compiled))
        {
            throw new InvalidOperationException($"No template registered with name '{name}'.");
        }

        return await RenderCompiledAsync(compiled, model, name);
    }

    private static Template Parse(string text, string name)
    {
        var template = Template.Parse(text);

        if (template.HasErrors)
        {
            throw new TemplateException($"Template '{name}' has parse errors: {string.Join("; ", template.Messages)}");
        }

        return template;
    }

    private async ValueTask<string> RenderCompiledAsync(Template template, object? model, string name)
    {
        var context = new TemplateContext { TemplateLoader = _loader };
        var scriptObject = new ScriptObject();

        if (model is not null)
        {
            scriptObject.Import(model);
        }

        context.PushGlobal(scriptObject);

        try
        {
            return await template.RenderAsync(context);
        }
        catch (ScriptRuntimeException ex)
        {
            throw new TemplateException($"Template '{name}' failed to render: {ex.Message}", ex);
        }
    }
}
