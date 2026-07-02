using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace SquidStd.Templating.Internal;

/// <summary>
/// Scriban <see cref="ITemplateLoader" /> that resolves <c>{{ include }}</c> names from the renderer's
/// registered template sources.
/// </summary>
internal sealed class NamedTemplateLoader : ITemplateLoader
{
    private readonly IReadOnlyDictionary<string, string> _sources;

    public NamedTemplateLoader(IReadOnlyDictionary<string, string> sources)
    {
        _sources = sources;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        => templateName;

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        if (!_sources.TryGetValue(templatePath, out var source))
        {
            throw new TemplateException($"Template '{templatePath}' is not registered (include).");
        }

        return source;
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        => new(Load(context, callerSpan, templatePath));
}
