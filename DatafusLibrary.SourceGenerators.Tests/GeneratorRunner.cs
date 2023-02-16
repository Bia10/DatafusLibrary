using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;

namespace DatafusLibrary.SourceGenerators.Tests;

public static class GeneratorRunner
{
    private static Compilation CreateCompilation(string source)
    {
        var syntaxTrees = new[]
        {
             CSharpSyntaxTree.ParseText(source, 
                 new CSharpParseOptions(LanguageVersion.Preview), string.Empty, Encoding.UTF8)
        };

        var references = new List<PortableExecutableReference>();
        var runtimeLibraries = DependencyContext.Default?.RuntimeLibraries;

        if (runtimeLibraries is not null)
        {
            references.AddRange(runtimeLibraries
                .Where(IsRuntimeLibraryGenerator)
                .Select(library => Assembly.Load(new AssemblyName(library.Name)))
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)));
        }

        references.Add(MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location));

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create(nameof(GeneratorRunner), syntaxTrees, references, options);

        return compilation;
    }

    public static GeneratorResult Run(string sourceCode, ISourceGenerator generators)
    {
        var compilation = CreateCompilation(sourceCode);

        var arrayOfGenerators = ImmutableArray.Create(generators);
        var arrayOfAdditionalTexts = ImmutableArray<AdditionalText>.Empty;
        var parseOptions = compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var driver = CSharpGeneratorDriver.Create(arrayOfGenerators, arrayOfAdditionalTexts, parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generatedCode = GetGeneratedCode(generators, outputCompilation);

        return new GeneratorResult(outputCompilation, diagnostics, generatedCode);
    }

    private static string GetGeneratedCode(ISourceGenerator generators, Compilation outputCompilation)
    {
        var syntaxTree = outputCompilation.SyntaxTrees.FirstOrDefault();

        if (syntaxTree is null)
            throw new InvalidOperationException();

        return SyntaxTreeIsOfGeneratorType(syntaxTree, generators.GetType().Name).ToString();
    }

    private static bool IsRuntimeLibraryGenerator(RuntimeLibrary library)
    {
        return library.Name.IndexOf("SourceGenerators", StringComparison.Ordinal) > -1;
    }

    private static bool SyntaxTreeIsOfGeneratorType(SyntaxTree syntaxTree, string generatorTypeName)
    {
        return syntaxTree.FilePath.IndexOf(generatorTypeName, StringComparison.Ordinal) > -1;
    }
}