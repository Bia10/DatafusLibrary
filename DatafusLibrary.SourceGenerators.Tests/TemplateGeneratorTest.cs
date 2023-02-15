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
        const string templateName = "BasicClass.scriban";
        const string packageName = "com.ankamagames.dofus.datacenter.world";

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var dirPath = Path.GetFullPath(Path.Combine(desktopPath, @".\Dofus2Botting\data\entities_json"));

        var worldPackageGroup = await EntityParser.GetEntityClassesPackageGroupByName(dirPath, packageName);
        var classesFromPackageGroup = EntityParser.GetClassesFromPackageGroup(worldPackageGroup);

        var templateString = TemplateLoader.LoadTemplate(templateName, typeof(TemplateLoader));
        var outputDir = Path.Combine(dirPath, packageName);

        Directory.CreateDirectory(outputDir);

        foreach (var basicClass in classesFromPackageGroup)
        {
            var generatedSource = TemplateGenerator.Execute(templateString, basicClass);

            var sourceFile = new GeneratedSourceFile(generatedSource, basicClass.ClassName);

            var finalPath = Path.Combine(outputDir, sourceFile.FileName + "generated.cs");

            await sourceFile.WriteToFile(finalPath);
        }
    }
}