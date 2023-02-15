using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace DatafusLibrary.SourceGenerators.Tests;

public class GeneratorResult
{
    public GeneratorResult(Compilation compilation, ImmutableArray<Diagnostic> diagnostics, string generatedCode)
    {
        Compilation = compilation;
        Diagnostics = diagnostics;
        GeneratedCode = generatedCode;
    }

    public Compilation Compilation { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public string GeneratedCode { get; }
}