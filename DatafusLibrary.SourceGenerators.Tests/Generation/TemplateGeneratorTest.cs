using System.Collections.Immutable;
using System.Diagnostics;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.SourceGenerators.Generators;
using DatafusLibrary.SourceGenerators.Templates;
using DatafusLibrary.SourceGenerators.Tests.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests.Generation;

public class TemplateGeneratorTest
{
    private readonly GenerationContext _generationContext;
    private readonly ITestOutputHelper _output;

    public TemplateGeneratorTest(ITestOutputHelper output)
    {
        _output = output;
        _generationContext = new GenerationContext(
            successSyntaxTrees: new List<SyntaxTree>(),
            failedSyntaxTrees: new List<SyntaxTree>(),
            generationResults: new List<GeneratorResult>(),
            inputTemplateName: "BasicClass.scriban",
            generatedSrcFileSuffix: ".generated.cs",
            generationOutputPath: "com.ankamagames.dofus.datacenter.world",
            outputAssemblyName: "com.ankamagames.dofus.datacenter.dll",
            jsonDataDirectoryPath: string.Empty)
        {
            JsonDataDirectoryPath = GenerationContext.SetInputDataPath()
        };
    }

    [Fact]
    public async Task GenerateFromTemplateTest()
    {
        var allBasicClasses = await EntityParser.GetAllBasicClassesFromDir(_generationContext.JsonDataDirectoryPath);

        var outputDir = GenerationContext.CreateOutputDir(_generationContext.JsonDataDirectoryPath,
            _generationContext.GenerationOutputPath);

        var templateString = TemplateLoader.LoadTemplate(_generationContext.InputTemplateName, typeof(TemplateLoader));

        GeneratorResult? generationResult = null;

        foreach (var basicClass in allBasicClasses)
        {
            if (basicClass.ClassName.Equals("LuaFormula"))
                continue;

            var generatedSource = TemplateGenerator.Execute(templateString, basicClass);

            var generatedSourceFile = new GeneratedSourceFile(generatedSource,
                basicClass.ClassName + _generationContext.GeneratedSrcFileSuffix);

            var srcFilePath = Path.Combine(outputDir, generatedSourceFile.FileName);

            await generatedSourceFile.WriteToFile(srcFilePath);

            generationResult = GeneratorRunner.Run(generatedSourceFile.SourceCode, new BasicClassGenerator());

            Debug.Assert(generationResult.Compilation.SyntaxTrees.Count().Equals(2));

            _output.WriteLine(generationResult.GetDiagnostics());

            _generationContext.GenerationResults.Add(generationResult);
            _generationContext.SuccessSyntaxTrees.Add(generationResult.Compilation.SyntaxTrees.Last());
        }

        var syntaxTrees = _generationContext.SuccessSyntaxTrees.DistinctBy(file => file.FilePath).ToList();
        var references = generationResult?.Compilation.References;

        var compilationOutputPath = Path.Combine(outputDir + "\\" + _generationContext.OutputAssemblyName);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var newCompilation =
            CSharpCompilation.Create(_generationContext.OutputAssemblyName, syntaxTrees, references, options);

        var arrayOfGenerators = ImmutableArray<BasicClassGenerator>.Empty;
        var arrayOfAdditionalTexts = ImmutableArray<AdditionalText>.Empty;
        var parseOptions = generationResult?.Compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var driver = CSharpGeneratorDriver.Create(arrayOfGenerators, arrayOfAdditionalTexts, parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(newCompilation, out var outputCompilation, out var diagnostics);

        var result = outputCompilation.Emit(compilationOutputPath);

        Debug.Assert(result.Success.Equals(true));

        if (result.Diagnostics.Any())
            _output.WriteLine(string.Join(Environment.NewLine,
                result.Diagnostics.Select(diagnostic => diagnostic.Location.ToString())));
    }
}