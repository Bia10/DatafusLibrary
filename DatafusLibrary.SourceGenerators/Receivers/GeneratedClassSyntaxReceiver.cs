using System.CodeDom.Compiler;
using DatafusLibrary.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DatafusLibrary.SourceGenerators.Receivers;

public class GeneratedClassSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        var attributeName = nameof(GeneratedCodeAttribute).Replace("Attribute", string.Empty);

        if (syntaxNode is ClassDeclarationSyntax classSyntax && classSyntax.HasAttribute(attributeName))
        {
            CandidateClasses.Add(classSyntax);
        }
    }
}