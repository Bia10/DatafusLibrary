using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DatafusLibrary.SourceGenerators.Extensions;

public static class Roslyn
{
    public static bool HaveAttribute(this ClassDeclarationSyntax classSyntax, string attributeName)
    {
        return classSyntax.AttributeLists.Count > 0 && classSyntax.AttributeLists
                   .SelectMany(attributeList => attributeList.Attributes
                   .Where(attributeSyntax => (attributeSyntax.Name as IdentifierNameSyntax)?.Identifier.Text == attributeName))
                   .Any();
    }
}