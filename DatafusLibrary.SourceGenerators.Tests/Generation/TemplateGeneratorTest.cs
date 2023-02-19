using System.Collections.Immutable;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.SourceGenerators.Generators;
using DatafusLibrary.SourceGenerators.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests.Generation;

public class TemplateGeneratorTest
{
    private readonly ITestOutputHelper _output;

    public TemplateGeneratorTest(ITestOutputHelper output)
    {
        _output = output;
    }

    private static string CreateOutputDir(string inputDataPath)
    {
        const string generationOutputPath = "com.ankamagames.dofus.datacenter.world";

        var outputDir = Path.Combine(inputDataPath, generationOutputPath);

        if (!Directory.Exists(outputDir))
        {
            var dirInfo = Directory.CreateDirectory(outputDir);

            return dirInfo.FullName;
        }

        return outputDir;
    }

    [Fact]
    public async Task GenerateFromTemplateTest()
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var dirPath = Path.GetFullPath(Path.Combine(desktopPath, @".\Dofus2Botting\data\entities_json"));

        var allBasicClasses = await EntityParser.GetAllBasicClassesFromDir(dirPath);

        var outputDir = CreateOutputDir(dirPath);

        var templateString = TemplateLoader.LoadTemplate("BasicClass.scriban", typeof(TemplateLoader));

        var successTrees = new List<SyntaxTree>();
        var failedTrees = new List<SyntaxTree>();

        GeneratorResult? generationResult = null;

        foreach (var basicClass in allBasicClasses)
        {
            if (basicClass.ClassName.Equals("LuaFormula"))
            {
                continue;
            }

            var generatedSource = TemplateGenerator.Execute(templateString, basicClass);

            var srcFile = new GeneratedSourceFile(generatedSource, basicClass.ClassName);

            var finalPath = Path.Combine(outputDir, srcFile.FileName + ".generated.cs");

            await srcFile.WriteToFile(finalPath);

            generationResult = GeneratorRunner.Run(srcFile.SourceCode, new BasicClassGenerator());

            if (generationResult is not null)
            {
                var diagnosticErrors = generationResult.Diagnostics
                    .Where(diagnostic => diagnostic.WarningLevel.Equals(0))
                    .ToList();

                if (diagnosticErrors.Any())
                {
                    _output.WriteLine(string.Join(Environment.NewLine, diagnosticErrors.Select(diagnostic => diagnostic.ToString())));
                    failedTrees.AddRange(generationResult.Compilation.SyntaxTrees);
                    continue;
                }

                successTrees.Add(generationResult.Compilation.SyntaxTrees.Last());
            }
        }

        const string assemblyName = "com.ankamagames.dofus.datacenter.dll";
        var syntaxTrees = successTrees.DistinctBy(file => file.FilePath).ToList();
        var references = generationResult?.Compilation.References;

        var compilationOutputPath = Path.Combine(outputDir + "\\" + assemblyName);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var newCompilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, options);

        var arrayOfGenerators = ImmutableArray<BasicClassGenerator>.Empty;
        var arrayOfAdditionalTexts = ImmutableArray<AdditionalText>.Empty;
        var parseOptions = generationResult?.Compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var driver = CSharpGeneratorDriver.Create(arrayOfGenerators, arrayOfAdditionalTexts, parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(newCompilation, out var outputCompilation, out var diagnostics);

        var result = outputCompilation.Emit(compilationOutputPath);

        if (result.Diagnostics.Any())
        {
            _output.WriteLine(string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Location.ToString())));
        }
    }
}