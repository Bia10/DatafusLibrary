using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DatafusLibrary.SourceGenerators.Generators;

[Generator]
public class BasicClassGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // init
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var root = context.Compilation.SyntaxTrees.First().GetRoot() as CompilationUnitSyntax;
        var namespaceSyntax = root?.Members.OfType<NamespaceDeclarationSyntax>().First();
        var programClassSyntax = namespaceSyntax.Members.OfType<ClassDeclarationSyntax>().First();
        var mainMethodSyntax = programClassSyntax.Members.OfType<PropertyDeclarationSyntax>().First();

        Console.WriteLine($"{namespaceSyntax}");
        Console.WriteLine($"{programClassSyntax}");
        Console.WriteLine($"{mainMethodSyntax}");

        // file has been generated -> loaded -> now roslyn can analyze it
        // and commit another pass of rebuilding the class with more information obtained
        AssembleClass(programClassSyntax, context.Compilation);
    }

    private void AssembleClass(ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation)
    {
    }
}
