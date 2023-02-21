using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DatafusLibrary.SourceGenerators.Tests.Models;

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

    public string GetDiagnostics()
    {
        var diagnostics = Diagnostics
            .Select(diagnostic => diagnostic.ToString())
            .ToList();

        return diagnostics.Any() ? string.Join(Environment.NewLine, diagnostics) : string.Empty;
    }

    public string GetDiagnosticsOfWarningLevel(int warningLevel)
    {
        var diagnostics = Diagnostics
            .Where(diagnostics => diagnostics.WarningLevel.Equals(warningLevel))
            .Select(diagnostic => diagnostic.ToString())
            .ToList();

        return diagnostics.Any() ? string.Join(Environment.NewLine, diagnostics) : string.Empty;
    }

    public string GetDiagnosticsBySeverity(DiagnosticSeverity diagnosticSeverity)
    {
        var diagnostics = Diagnostics
            .Where(diagnostics => diagnostics.Severity.Equals(diagnosticSeverity))
            .Select(diagnostic => diagnostic.ToString())
            .ToList();

        return diagnostics.Any() ? string.Join(Environment.NewLine, diagnostics) : string.Empty;
    }
}