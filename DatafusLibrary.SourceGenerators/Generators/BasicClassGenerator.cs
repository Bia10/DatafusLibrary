using System.Text;
using DatafusLibrary.SourceGenerators.Receivers;
using DatafusLibrary.SourceGenerators.Sharp;
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

    private GeneratedSourceFile GenerateClass(ClassDeclarationSyntax classSyntax, GeneratorExecutionContext context)
    {
        var classModel = GetClassModel(classSyntax, context.Compilation);

        var templateString = TemplateLoader.LoadTemplate("BasicClass.scriban");

        var result = TemplateGenerator.Execute(templateString, classModel);

        return new GeneratedSourceFile(result, classModel.ClassName);
    }

    private BasicClass GetClassModel(ClassDeclarationSyntax classSyntax, Compilation compilation)
    {
        var classRoot = classSyntax.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();;
        var namespaceSyntax = classRoot?.Members.OfType<NamespaceDeclarationSyntax>().First();
        var classSemanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
        var classSymbol = classSemanticModel.GetDeclaredSymbol(classSyntax);

        var classModel = new BasicClass
        {
            // TODO:
        };

        return classModel;
    }
}
