using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SquidStd.Tests.Plugin.Support;

internal static class PluginAssemblyFactory
{
    public static string CompilePluginAssembly(string directory, string pluginId)
    {
        var source = $$"""
                       using DryIoc;
                       using SquidStd.Plugin.Abstractions.Data;
                       using SquidStd.Plugin.Abstractions.Interfaces.Plugins;

                       public sealed class GeneratedPlugin : ISquidStdPlugin
                       {
                           public PluginMetadata Metadata { get; } = new()
                           {
                               Id = "{{pluginId}}",
                               Name = "Generated Plugin",
                               Version = new(1, 0, 0),
                               Author = "Tests"
                           };

                           public void Configure(IContainer container, PluginContext context)
                               => container.RegisterInstance("{{pluginId}}", serviceKey: "plugin-marker");
                       }
                       """;

        return Emit(directory, "generated-plugin.dll", source);
    }

    public static string CompileNonPluginAssembly(string directory)
        => Emit(directory, "not-a-plugin.dll", "public sealed class NotAPlugin { }");

    private static string Emit(string directory, string fileName, string source)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            // Earlier tests in this run may have loaded plugin assemblies from temp
            // directories that have since been deleted (Scan uses Assembly.LoadFrom into
            // the default ALC, which never unloads). Skip those so a stale Location doesn't
            // blow up MetadataReference.CreateFromFile for unrelated tests.
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location) &&
                                File.Exists(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilation = CSharpCompilation.Create(
            $"GeneratedPluginAssembly_{Guid.NewGuid():N}",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var path = Path.Combine(directory, fileName);
        var result = compilation.Emit(path);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, result.Diagnostics.Select(d => d.ToString()))
            );
        }

        return path;
    }
}
