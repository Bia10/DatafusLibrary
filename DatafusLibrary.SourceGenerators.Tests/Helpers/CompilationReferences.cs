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

        var compileLibraries = DependencyContext.Default?.CompileLibraries
             .Where(compileLib => compileLib.Type.Equals("referenceassembly", StringComparison.Ordinal))
             .Where(compileLib => !string.IsNullOrEmpty(compileLib.Path))
             .ToList();

        if (compileLibraries is null || !compileLibraries.Any())
            throw new InvalidOperationException("No suitable references in compile libraries found!");

        try
        {
            references.AddRange(compileLibraries
                .SelectMany(compileLibrary => compileLibrary.ResolveReferencePaths())
                .Select(libraryPath => MetadataReference.CreateFromFile(libraryPath)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return references;
    }

    private static bool IsLibrarySourceGenerator(Library library)
    {
        return library.Name.IndexOf("SourceGenerators", StringComparison.Ordinal) > -1;
    }
}