using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DatafusLibrary.SourceGenerators.Extensions;

public static class Roslyn
{
    private static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
    {
        var currentType = type;

        while (currentType is not null)
        {
            yield return currentType;

            currentType = currentType.BaseType;
        }
    }

    public static CompilationUnitSyntax GetCompilationUnit(this ClassDeclarationSyntax syntaxNode)
    {
        return syntaxNode.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault()
               ?? throw new InvalidOperationException(
                   $"Compilation unit not found for sytaxNode: {syntaxNode.Identifier.ToFullString()}");
    }

    public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol type)
    {
        return type.GetBaseTypesAndThis().SelectMany(typeSymbol => typeSymbol.GetMembers());
    }

    public static string GetClassName(this ClassDeclarationSyntax classSyntax)
    {
        return classSyntax.Identifier.Text;
    }

    public static string GetClassModifier(this ClassDeclarationSyntax classSyntax)
    {
        return classSyntax.Modifiers.ToFullString().Trim();
    }

    public static bool HasAttribute(this ClassDeclarationSyntax classSyntax, string attributeName)
    {
        return classSyntax.AttributeLists.Count > 0 && classSyntax.AttributeLists
                   .SelectMany(attributeList => attributeList.Attributes
                   .Where(attributeSyntax => (attributeSyntax.Name as IdentifierNameSyntax)?.Identifier.Text == attributeName))
                   .Any();
    }

    public static string GetNamespace(this CompilationUnitSyntax classSyntax)
    {
        var namespaceDeclaration = classSyntax.ChildNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

        return namespaceDeclaration is not null ? namespaceDeclaration.Name.ToString() : string.Empty;
    }

    public static List<string> GetUsingDirectives(this CompilationUnitSyntax classSyntax)
    {
        return classSyntax.ChildNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(usingDirective => usingDirective.Name.ToString())
            .ToList();
    }
}