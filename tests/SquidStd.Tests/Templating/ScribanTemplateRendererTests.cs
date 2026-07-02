using SquidStd.Templating;
using SquidStd.Templating.Services;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Templating;

public class ScribanTemplateRendererTests
{
    [Fact]
    public void Register_MalformedTemplate_Throws()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);

        Assert.Throws<TemplateException>(() => renderer.Register("bad", "{{ for x in }}"));
    }

    [Fact]
    public async Task RegisterThenRenderByName_Works()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);
        renderer.Register("greet", "Hi {{ user.name }}");

        var result = await renderer.RenderByNameAsync("greet", new { User = new { Name = "squid" } });

        Assert.Equal("Hi squid", result);
    }

    [Fact]
    public async Task RenderAsync_EmptyTemplate_Throws()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);

        await Assert.ThrowsAsync<ArgumentException>(async () => await renderer.RenderAsync(string.Empty, null));
    }

    [Fact]
    public async Task RenderAsync_MalformedTemplate_Throws()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);

        await Assert.ThrowsAsync<TemplateException>(async () => await renderer.RenderAsync("{{ for x in }}", null));
    }

    [Fact]
    public async Task RenderAsync_RendersModel()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);

        var result = await renderer.RenderAsync("Hi {{ user.name }}", new { User = new { Name = "squid" } });

        Assert.Equal("Hi squid", result);
    }

    [Fact]
    public async Task RenderByNameAsync_UnknownName_Throws()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await renderer.RenderByNameAsync("nope", null));
    }

    [Fact]
    public async Task StartAsync_AutoLoadsTemplatesFromDirectory()
    {
        using var temp = new TempDirectory();
        var emails = Path.Combine(temp.Path, "templates", "emails");
        Directory.CreateDirectory(emails);
        await File.WriteAllTextAsync(Path.Combine(emails, "welcome.tmpl"), "Welcome {{ user.name }}");

        var renderer = NewRenderer(temp.Path);
        await renderer.StartAsync();

        var result = await renderer.RenderByNameAsync("emails/welcome", new { User = new { Name = "squid" } });

        Assert.Equal("Welcome squid", result);
    }

    [Fact]
    public async Task Include_ResolvesRegisteredTemplate()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);
        renderer.Register("partials/header", "HEADER");
        renderer.Register("page", "{{ include 'partials/header' }} BODY");

        var result = await renderer.RenderByNameAsync("page", null);

        Assert.Equal("HEADER BODY", result);
    }

    [Fact]
    public async Task Include_FromInlineTemplate_Works()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);
        renderer.Register("partials/header", "HEADER");

        var result = await renderer.RenderAsync("{{ include 'partials/header' }}!", null);

        Assert.Equal("HEADER!", result);
    }

    [Fact]
    public async Task Include_WithModel_BindsModel()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);
        renderer.Register("partials/hello", "Hi {{ user.name }}");
        renderer.Register("page", "{{ include 'partials/hello' }}");

        var result = await renderer.RenderByNameAsync("page", new { User = new { Name = "squid" } });

        Assert.Equal("Hi squid", result);
    }

    [Fact]
    public async Task Include_MissingTemplate_Throws()
    {
        using var temp = new TempDirectory();
        var renderer = NewRenderer(temp.Path);

        await Assert.ThrowsAsync<TemplateException>(
            async () => await renderer.RenderAsync("{{ include 'nope' }}", null)
        );
    }

    [Fact]
    public async Task Include_AutoLoadedFromDisk_Works()
    {
        using var temp = new TempDirectory();
        var templatesDir = Path.Combine(temp.Path, "templates");
        Directory.CreateDirectory(Path.Combine(templatesDir, "partials"));
        await File.WriteAllTextAsync(Path.Combine(templatesDir, "partials", "header.tmpl"), "HEADER");
        await File.WriteAllTextAsync(
            Path.Combine(templatesDir, "page.tmpl"),
            "{{ include 'partials/header' }} BODY"
        );

        var renderer = NewRenderer(temp.Path);
        await renderer.StartAsync();

        var result = await renderer.RenderByNameAsync("page", null);

        Assert.Equal("HEADER BODY", result);
    }

    private static ScribanTemplateRenderer NewRenderer(string root)
        => new(new(root, []));
}
