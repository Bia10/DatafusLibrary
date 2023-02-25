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

    public static IEnumerable<MetadataReference> GetPredefined(bool useRuntime = false)
    {
        var refDirectory = "C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref\\7.0.3\\ref\\net7.0\\";

        if (OperatingSystem.IsLinux())
            refDirectory = "/usr/share/dotnet" + "\\packs\\Microsoft.NETCore.App.Ref\\7.0.3\\ref\\net7.0\\"
                .Replace('\\', '/');

        var requiredAssemblies = new[]
        {
            "System.dll",
            "System.Runtime.dll",
            "System.Collections.dll"
        };

        var assemblyPaths = useRuntime switch
        {
            true => Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"),
            false => Directory.GetFiles(refDirectory, "*.dll")
        };

        var reqAssembliesPath = (
            from requiredAssembly in requiredAssemblies
            from assemblyPath in assemblyPaths
            where assemblyPath.Replace(refDirectory, string.Empty).Equals(requiredAssembly, StringComparison.Ordinal)
            select assemblyPath).ToList();

        var metadataReferences = reqAssembliesPath
            .Select(libraryPath => MetadataReference.CreateFromFile(libraryPath))
            .ToList();

        if (metadataReferences is null || !metadataReferences.Any())
            throw new InvalidOperationException("No suitable references in compile libraries found!");

        return metadataReferences;
    }
}