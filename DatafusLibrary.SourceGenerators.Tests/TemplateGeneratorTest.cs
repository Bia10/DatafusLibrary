using DatafusLibrary.Core.Parsers;
using DatafusLibrary.SourceGenerators.Generators;
using DatafusLibrary.SourceGenerators.Templates;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests;

public class TemplateGeneratorTest
{
    private readonly ITestOutputHelper _output;

    public TemplateGeneratorTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task GenerateFromTemplateTest()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory);
        var solutionDirectory = projectDirectory?.Parent?.Parent?.Parent;

        var path = solutionDirectory + @"\DatafusLibrary.TestConsole\MockData\Areas.json";

        const string templateName = "BasicClass.scriban";

        var classModels = await EntityParser.ParseEntityToBasicClass(path);

        var templateString = TemplateLoader.LoadTemplate(templateName, typeof(TemplateLoader));

        foreach (var generatedSource in classModels.Select(classModel =>
                     TemplateGenerator.Execute(templateString, classModel)))
        {
            _output.WriteLine(generatedSource);
        }
    }
}