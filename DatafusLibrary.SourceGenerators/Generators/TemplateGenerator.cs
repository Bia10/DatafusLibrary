using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Scriban;

namespace DatafusLibrary.SourceGenerators.Generators;

public static class TemplateGenerator
{
    public static string Execute(string templateString, object classModel)
    {
        var template = Template.Parse(templateString);

        if (template.HasErrors)
        {
            var errors = string.Join(" | ", template.Messages.Select(x => x.Message));
            throw new InvalidOperationException($"Template parsing errors: {errors}");
        }

        var renderedTemplate = template.Render(classModel, member => member.Name);

        var syntaxResult = SyntaxFactory.ParseCompilationUnit(renderedTemplate)
            .NormalizeWhitespace()
            .GetText()
            .ToString();

        return syntaxResult;
    }
}