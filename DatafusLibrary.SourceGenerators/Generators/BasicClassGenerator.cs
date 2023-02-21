using System.CodeDom.Compiler;
using System.Text;
using DatafusLibrary.SourceGenerators.Extensions;
using DatafusLibrary.SourceGenerators.Models;
using DatafusLibrary.SourceGenerators.Models.Sharp;
using DatafusLibrary.SourceGenerators.Models.Sharp.Descriptors;
using DatafusLibrary.SourceGenerators.Receivers;
using DatafusLibrary.SourceGenerators.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
            foreach (var classSyntax in classSyntaxReceiver.CandidateClasses)
            {
                // has same class name and property name
                if (classSyntax.GetClassName().Equals("LuaFormula"))
                    continue;

                var source = GenerateClass(classSyntax, context);

                context.AddSource(source.FileName, SourceText.From(source.SourceCode, Encoding.UTF8));
            }
    }

    private static GeneratedSourceFile GenerateClass(ClassDeclarationSyntax classSyntax,
        GeneratorExecutionContext context)
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

        var properties = classSyntax.GetMembersOfKind(SyntaxKind.PropertyDeclaration)
            .Select(member => member as PropertyDeclarationSyntax)
            .Select(property => new PropertyDescriptor
            {
                Name = property is not null ? property.Identifier.ValueText : string.Empty,
                Type = property is not null ? property.Type.ToFullString() : string.Empty
            }).ToList();

        var namespaces = classSyntax.GetNamespacesFromSymbols(classSemanticModel, compilation);

        var attributeName = nameof(GeneratedCodeAttribute).Replace("Attribute", string.Empty);

        var classModel = new BasicClass
        {
            Namespace = classRoot.GetNamespace(),
            Usings = namespaces,
            ClassAttributes = attributeName,
            ClassAccessModifier = string.IsNullOrEmpty(classSyntax.GetClassModifier())
                ? "public"
                : classSyntax.GetClassModifier(),
            ClassName = classSyntax.GetClassName(),
            Properties = properties,
            ConstructorParameters = PropertiesToConstructorParams(properties),
            InjectedProperties = PropertiesToInjectedProperties(properties)
        };

        return classModel;
    }

    private static List<PropertyAssignDescriptor> PropertiesToInjectedProperties(
        IEnumerable<PropertyDescriptor> properties)
    {
        var injectedProperties = properties
            .Select(property => new PropertyAssignDescriptor
            {
                Destination = property.Name.EscapeCSharpKeywords(),
                Type = property.Type.ToString().EscapeCSharpKeywords(),
                Source = property.Name.EscapeCSharpKeywords(true)
            }).ToList();

        return injectedProperties;
    }

    private static List<ParameterDescriptor> PropertiesToConstructorParams(IEnumerable<PropertyDescriptor> properties)
    {
        var injectedProperties = properties
            .Select(property => new ParameterDescriptor
            {
                Name = property.Name.EscapeCSharpKeywords(true),
                Type = property.Type.ToString().EscapeCSharpKeywords()
            }).ToList();

        return injectedProperties;
    }
}