using System.CodeDom.Compiler;
using System.Text;
using DatafusLibrary.SourceGenerators.Extensions;
using DatafusLibrary.SourceGenerators.Receivers;
using DatafusLibrary.SourceGenerators.Sharp;
using DatafusLibrary.SourceGenerators.Sharp.Descriptors;
using DatafusLibrary.SourceGenerators.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
        if (context.SyntaxReceiver is GeneratedClassSyntaxReceiver classSyntaxReceiver)
        {
            foreach (var classSyntax in classSyntaxReceiver.CandidateClasses)
            {
                foreach (var memberDeclarationSyntax in classSyntax.Members)
                {
                    Console.WriteLine($"{memberDeclarationSyntax}");
                }

                var source = GenerateClass(classSyntax, context);

                context.AddSource(source.FileName, SourceText.From(source.SourceCode, Encoding.UTF8));
            }
        }
    }

    private static GeneratedSourceFile GenerateClass(ClassDeclarationSyntax classSyntax, GeneratorExecutionContext context)
    {
        var classModel = GetClassModel(classSyntax, context.Compilation);

        var templateString = TemplateLoader.LoadTemplate("BasicClass.scriban");

        var result = TemplateGenerator.Execute(templateString, classModel);

        return new GeneratedSourceFile(result, classModel.ClassName);
    }

    private static BasicClass GetClassModel(ClassDeclarationSyntax classSyntax, Compilation compilation)
    {
        var classRoot = classSyntax.GetCompilationUnit();
        var classSemanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
        var classSymbol = classSemanticModel.GetDeclaredSymbol(classSyntax);

        var attributeName = nameof(GeneratedCodeAttribute).Replace("Attribute", string.Empty);

        var classModel = new BasicClass
        {
            ClassBase = classSyntax.GetClassName(),
            ClassName = $"{classSyntax.GetClassName()}{attributeName}",
            ClassModifier = classSyntax.GetClassModifier(),
            Usings = classRoot.GetUsingDirectives(),
            Namespace = classRoot.GetNamespace(),
            ConstructorParameters = GetDefaultConstructorProperties(classSymbol.ContainingType),
            InjectedProperties = GetInjectedProperties(classSymbol.ContainingType),
        };

        return classModel;
    }

    private static List<ParameterDescriptor> GetDefaultConstructorProperties(ITypeSymbol classSymbol)
    {
        var constructorProperties = classSymbol.GetAllMembers()
            .Where(symbol => symbol.Kind.Equals(SymbolKind.Property))
            .OfType<IPropertySymbol>()
            .Select(property => new ParameterDescriptor
            {
                Name = property.Name,
                Type = property.Type.ToString(),
            }).ToList();

        return constructorProperties;
    }

    private static List<PropertyAssignDescriptor> GetInjectedProperties(ITypeSymbol classSymbol)
    {
        var injectedProperties = classSymbol.GetAllMembers()
            .Where(symbol => symbol.Kind.Equals(SymbolKind.Property))
            .OfType<IPropertySymbol>()
            .Select(property => new PropertyAssignDescriptor
            {
                Destination = property.Name,
                Type = property.Type.ToString(),
                Source = property.Name
            }).ToList();

        return injectedProperties;
    }

    private static List<ParameterDescriptor>? GetConstructor(INamedTypeSymbol classSymbol)
    {
        var baseConstructor = classSymbol.Constructors
            .OrderByDescending(constructorSymbol => constructorSymbol.Parameters.Length)
            .FirstOrDefault();

        var parList = baseConstructor?.Parameters
            .Select(property => new ParameterDescriptor
            {
                Name = property.Name,
                Type = property.Type.ToString()
            }).ToList();

        return parList;
    }
}
