using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;

namespace DatafusLibrary.SourceGenerators.Tests.Helpers;

public static class CompilationReferences
{
    public static IEnumerable<MetadataReference> GetRuntimeReferences()
    {
        var references = new List<MetadataReference>();

        var runtimeLibraries = DependencyContext.Default?.RuntimeLibraries;
        if (runtimeLibraries is not null && runtimeLibraries.Any())
        {
            references.AddRange(runtimeLibraries
                .Select(runtimeLibrary => Assembly.Load(new AssemblyName(runtimeLibrary.Name)))
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)));
        }

        return references;
    }

    public static IEnumerable<MetadataReference> GetCompileReferences()
    {
        var references = new List<MetadataReference>();

        var compileLibraries = DependencyContext.Default?.CompileLibraries;
        if (compileLibraries is not null && compileLibraries.Any())
        {
            references.AddRange(compileLibraries
                .SelectMany(compileLibrary => compileLibrary.ResolveReferencePaths())
                .Select(libraryPath => MetadataReference.CreateFromFile(libraryPath)));
        }

        return references;
    }

    private static bool IsLibrarySourceGenerator(Library library)
    {
        return library.Name.IndexOf("SourceGenerators", StringComparison.Ordinal) > -1;
    }
}