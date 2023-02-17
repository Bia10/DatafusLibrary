using System.CodeDom.Compiler;
using System.Text;
using DatafusLibrary.SourceGenerators.Extensions;
using DatafusLibrary.SourceGenerators.Receivers;
using DatafusLibrary.SourceGenerators.Sharp;
using DatafusLibrary.SourceGenerators.Sharp.Descriptors;
using DatafusLibrary.SourceGenerators.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ParameterDescriptor = DatafusLibrary.SourceGenerators.Sharp.Descriptors.ParameterDescriptor;

namespace DatafusLibrary.SourceGenerators.Generators;

[Generator]
public class BasicClassGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new GeneratedClassSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is GeneratedClassSyntaxReceiver classSyntaxReceiver)
        {
            foreach (var classSyntax in classSyntaxReceiver.CandidateClasses)
            {
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

        var properties = classSyntax.Members
            .Where(member => member.IsKind(SyntaxKind.PropertyDeclaration))
            .Select(member => member as PropertyDeclarationSyntax)
            .Select(property => new PropertyDescriptor
            {
                Name = property.Identifier.ValueText,
                Type = property.Type.ToFullString()
            }).ToList();

        var requiredNamespaces = new List<string>();

        var symbols =  classSyntax.GetAllSymbols(classSemanticModel);

        foreach (var (symbol, typeSymbol) in symbols)
        {
            if (symbol is not null)
            {
                Console.WriteLine($"Symbol name: {symbol.Name} namespace: {symbol.ContainingNamespace}  module: {symbol.ContainingModule}");
                requiredNamespaces.Add(symbol.ContainingNamespace.ToString());
            }

            var namespaceOfCollection = typeSymbol.GetNamespaceFromMetadataName(compilation);

            if (!string.IsNullOrEmpty(namespaceOfCollection))
            {
                Console.WriteLine($"{namespaceOfCollection}");
                requiredNamespaces.Add(namespaceOfCollection);
            }
        }

        var attributeName = nameof(GeneratedCodeAttribute).Replace("Attribute", string.Empty);

        var classModel = new BasicClass
        {
            Namespace = classRoot.GetNamespace(),
            Usings = requiredNamespaces.Distinct().ToList(),
            ClassAttributes = attributeName,
            ClassAccessModifier = classSyntax.GetClassModifier(),
            ClassName = classSyntax.GetClassName(),
            Properties = properties,
            ConstructorParameters = PropertiesToConstructorParams(properties),
            InjectedProperties = PropertiesToInjectedProperties(properties)
        };

        return classModel;
    }

    private static List<PropertyAssignDescriptor> PropertiesToInjectedProperties(IEnumerable<PropertyDescriptor> properties)
    {
        var injectedProperties = properties
            .Select(property => new PropertyAssignDescriptor
            {
                Destination = property.Name,
                Type = property.Type.ToString(),
                Source = property.Name
            }).ToList();

        return injectedProperties;
    }

    private static List<ParameterDescriptor> PropertiesToConstructorParams(IEnumerable<PropertyDescriptor> properties)
    {
        var injectedProperties = properties
            .Select(property => new ParameterDescriptor
            {
                Name = property.Name,
                Type = property.Type.ToString()
            }).ToList();

        return injectedProperties;
    }
}
