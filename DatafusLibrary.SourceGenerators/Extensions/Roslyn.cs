using DatafusLibrary.SourceGenerators.Sharp.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DatafusLibrary.SourceGenerators.Extensions;

public static class Roslyn
{
    private static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol typeSymbol)
    {
        var currentType = typeSymbol;

        while (currentType is not null)
        {
            yield return currentType;

            currentType = currentType.BaseType;
        }
    }

    public static string GetTypeFullName(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType.Equals(SpecialType.None)
            ? typeSymbol.ToDisplayString()
            : typeSymbol.SpecialType.ToString().Replace("_", ".");
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

    public static IEnumerable<IPropertySymbol> GetClassProperties(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetAllMembers()
            .Where(symbol => symbol.Kind.Equals(SymbolKind.Property))
            .OfType<IPropertySymbol>().ToList();
    }

    public static List<PropertyDescriptor> GetClassPropertiesFromAllSymbols(this IEnumerable<ITypeSymbol> typeSymbols)
    {
        return typeSymbols
            .Where(symbol => symbol.Kind.Equals(SymbolKind.NamedType))
            .Select(property => new PropertyDescriptor
            {
                Name = property.Name,
                Type = property.BaseType.ToDisplayString()
            }).ToList();
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

    public static void PrintClassMembers(this ClassDeclarationSyntax classSyntax)
    {
        var classMembers = classSyntax.Members;

        Console.WriteLine($"Class contains: {classMembers.Count} members.");

        foreach (var member in classMembers)
        {
            Console.WriteLine($"AttributeList: {member.AttributeLists.ToFullString()}");
            Console.WriteLine($"Modifier: {member.Modifiers.ToFullString()}");
            Console.WriteLine($"Member syntax kind: {member.Kind()}");
        }
    }

    public static TypeSyntax AsTypeSyntax(this Type type)
    {
        var name = type.Name.Replace( '+', '.' );

        if (type.IsGenericType) 
        {
            name = name.Substring(0, name.IndexOf("`", StringComparison.Ordinal));

            var genericArgs = type.GetGenericArguments();
            var genericArgsTypeSyntax = genericArgs.Select(AsTypeSyntax);
            var identifier = SyntaxFactory.Identifier(name);
            var arguments = SyntaxFactory.SeparatedList(genericArgsTypeSyntax);
            var typeArgumentList = SyntaxFactory.TypeArgumentList(arguments);

            return SyntaxFactory.GenericName(identifier, typeArgumentList);
        }

        return SyntaxFactory.ParseTypeName(name);
    }

    public static bool IsPossibleArrayGenericInterface(this ITypeSymbol typesSymbol)
    {
        if (typesSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        namedTypeSymbol = namedTypeSymbol.OriginalDefinition;

        var specialType = namedTypeSymbol.SpecialType;

        if (specialType is SpecialType.System_Collections_Generic_IList_T or
            SpecialType.System_Collections_Generic_ICollection_T or
            SpecialType.System_Collections_Generic_IEnumerable_T or 
            SpecialType.System_Collections_Generic_IReadOnlyList_T or 
            SpecialType.System_Collections_Generic_IReadOnlyCollection_T)
        {
            return true;
        }

        return false;
    }

    public static string GetNamespaceFromMetadataName(this ITypeSymbol typeSymbol, Compilation compilation)
    {
        // number at end specifies number of T args, 1 = List<T>, ...
        if (typeSymbol.MetadataName.Equals("List`1"))
        {
            var iListTypeSymbol = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);

            var typeNamespace = iListTypeSymbol.ContainingNamespace.ToString();

            return typeNamespace;
        }

        return string.Empty;
    }

    public static IEnumerable<(ISymbol? Symbol, ITypeSymbol typeSymbol)> GetAllSymbols(this ClassDeclarationSyntax classSyntax, SemanticModel classSemanticModel)
    {
        var noDuplicatesSymbol = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var noDuplicateTypeSymbol = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var node in classSyntax.DescendantNodesAndSelf())
        {
            switch (node.Kind())
            {
                case SyntaxKind.GenericName:
                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.IdentifierName:
                case SyntaxKind.TypeArgumentList:
                case SyntaxKind.ParameterList:
                case SyntaxKind.AttributeList:
                case SyntaxKind.Attribute:
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.Block:
                case SyntaxKind.AccessorList:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                    continue;
                default:
                    var typeSymbol = classSemanticModel.GetTypeInfo(node).Type;
                    var symbol = classSemanticModel.GetSymbolInfo(node);

                    if (symbol.Symbol is not null && typeSymbol is not null)
                    {
                        if (noDuplicatesSymbol.Add(symbol.Symbol))
                            yield return (symbol.Symbol, typeSymbol);
                    }
                    else if (symbol.Symbol is null && typeSymbol is not null)
                    {
                        if(noDuplicateTypeSymbol.Add(typeSymbol))
                            yield return (null, typeSymbol);

                        Console.WriteLine($"No symbol found for symbolInfo: {node.Kind()} rawKind: {node.RawKind} nodeName: {node.ToFullString()}");
                    }

                    break;
            }
        }
    }
}