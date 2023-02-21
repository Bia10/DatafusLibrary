using System.Collections.Immutable;
using System.Text;
using DatafusLibrary.SourceGenerators.Tests.Helpers;
using DatafusLibrary.SourceGenerators.Tests.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

        var references = CompilationReferences.GetPredefined();
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create(nameof(GeneratorRunner), syntaxTrees, references, options);

        return compilation;
    }

    public static GeneratorResult Run(string sourceCode, ISourceGenerator generator)
    {
        var compilation = CreateCompilation(sourceCode);

        var arrayOfGenerators = ImmutableArray.Create(generator);
        var arrayOfAdditionalTexts = ImmutableArray<AdditionalText>.Empty;
        var parseOptions = compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var driver = CSharpGeneratorDriver.Create(arrayOfGenerators, arrayOfAdditionalTexts, parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generatedCode = GetGeneratedCode(generator, outputCompilation);

        return new GeneratorResult(outputCompilation, diagnostics, generatedCode);
    }

    private static string GetGeneratedCode(ISourceGenerator generator, Compilation outputCompilation)
    {
        var syntaxTree = outputCompilation.SyntaxTrees.LastOrDefault();

        if (syntaxTree is null || string.IsNullOrEmpty(syntaxTree.FilePath))
            throw new InvalidOperationException();

        return SyntaxTreeIsOfGeneratorType(syntaxTree, generator)
            ? syntaxTree.GetText().ToString()
            : string.Empty;
    }

    private static bool SyntaxTreeIsOfGeneratorType(SyntaxTree syntaxTree, ISourceGenerator generator)
    {
        var generatorTypeName = generator.GetType().Name;
        return syntaxTree.FilePath.IndexOf(generatorTypeName, StringComparison.Ordinal) > -1;
    }
}