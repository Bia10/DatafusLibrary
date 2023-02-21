using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;

namespace DatafusLibrary.SourceGenerators.Tests.Helpers;

public static class CompilationReferences
{
    public static IEnumerable<MetadataReference> GetRuntimeReferences()
    {
        var references = new List<MetadataReference>();

        var runtimeLibraries = DependencyContext.Default?.RuntimeLibraries;
        if (runtimeLibraries is not null && runtimeLibraries.Any())
            references.AddRange(runtimeLibraries
                .Select(runtimeLibrary => Assembly.Load(new AssemblyName(runtimeLibrary.Name)))
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)));

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

    public static IEnumerable<MetadataReference> GetPredefined()
    {
        var requiredLibs = new[]
        {
            "netstandard.dll",
            "mscorlib.dll",
            "System.dll",
            "System.Runtime.dll",
            "System.Collections.dll",
        };

        var compileLibraries = DependencyContext.Default?.CompileLibraries
            .Where(compileLib => compileLib.Type.Equals("referenceassembly", StringComparison.Ordinal))
            .Where(compileLib => !string.IsNullOrEmpty(compileLib.Path))
            .ToList();


        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
        var refAssemblies = Directory.GetFiles("C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref\\7.0.3\\ref\\net7.0\\", "*.dll");


        var matchingRequest = refAssemblies
            .Where(assembly => assembly.EndsWith(requiredLibs[0]) ||
                               assembly.EndsWith(requiredLibs[1]) ||
                               assembly.EndsWith(requiredLibs[2]) ||
                               assembly.EndsWith(requiredLibs[3]) ||
                               assembly.EndsWith(requiredLibs[4]));

        var paths = new List<string>(matchingRequest);

        var metadataReferences = paths
            .Select(libraryPath => MetadataReference.CreateFromFile(libraryPath))
            .ToList();

        if (metadataReferences is null || !metadataReferences.Any())
            throw new InvalidOperationException("No suitable references in compile libraries found!");


        return metadataReferences.DistinctBy(meta => meta.FilePath);
    }


    private static bool IsLibrarySourceGenerator(Library library)
    {
        return library.Name.IndexOf("SourceGenerators", StringComparison.Ordinal) > -1;
    }
}