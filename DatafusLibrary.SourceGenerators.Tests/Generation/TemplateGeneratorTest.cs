using System.Collections.Immutable;
using System.Diagnostics;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.SourceGenerators.Generators;
using DatafusLibrary.SourceGenerators.Templates;
using DatafusLibrary.SourceGenerators.Tests.Helpers.NLog;
using DatafusLibrary.SourceGenerators.Tests.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NLog;
using NLog.Config;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests.Generation;

public class GeneratorTestFixture : IDisposable
{
    public readonly ILogger Logger;
    public GenerationContext? GenerationContext;
    public ITestOutputHelper? TestOutput;

    public GeneratorTestFixture()
    {
        var logFactory = new LogFactory();
        logFactory.ThrowExceptions = true;
        var configuration = new LoggingConfiguration();
        var testOutputTarget = new TestOutputTarget();

        if (TestOutput is not null)
            testOutputTarget.Add(TestOutput, nameof(TemplateGeneratorTest));

        configuration.AddRuleForAllLevels(testOutputTarget, nameof(TemplateGeneratorTest));
        logFactory.Configuration = configuration;

        Logger = logFactory.GetLogger(nameof(TemplateGeneratorTest));
        Logger.Info("GeneratorTestFixture Init!");

        GenerationContext = new GenerationContext(
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

    public void Dispose()
    {
        Logger.Factory.Dispose();
        GenerationContext = null;
        GC.SuppressFinalize(this);
    }
}

public class TemplateGeneratorTest : IClassFixture<GeneratorTestFixture>
{
    private readonly GeneratorTestFixture _fixture;

    public TemplateGeneratorTest(ITestOutputHelper? iTestOutputHelper, GeneratorTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.TestOutput = iTestOutputHelper;
    }

    [Fact]
    public async Task GenerateFromTemplateTest()
    {
        if (_fixture.GenerationContext is null)
            throw new ArgumentNullException(nameof(_fixture.GenerationContext));

        var allBasicClasses =
            await EntityParser.GetAllBasicClassesFromDir(_fixture.GenerationContext.JsonDataDirectoryPath);
        var outputDir = GenerationContext.CreateOutputDir(_fixture.GenerationContext.JsonDataDirectoryPath,
            _fixture.GenerationContext.GenerationOutputPath);
        var templateString =
            TemplateLoader.LoadTemplate(_fixture.GenerationContext.InputTemplateName, typeof(TemplateLoader));

        GeneratorResult? generationResult = null;

        foreach (var basicClass in allBasicClasses)
        {
            if (basicClass.ClassName.Equals("LuaFormula", StringComparison.Ordinal))
                continue;

            var generatedSource = TemplateGenerator.Execute(templateString, basicClass);
            var generatedSourceFile = new GeneratedSourceFile(generatedSource,
                basicClass.ClassName + _fixture.GenerationContext.GeneratedSrcFileSuffix);
            var srcFilePath = Path.Combine(outputDir, generatedSourceFile.FileName);

            await generatedSourceFile.WriteToFile(srcFilePath);

            generationResult = GeneratorRunner.Run(generatedSourceFile.SourceCode, new BasicClassGenerator());

            Debug.Assert(generationResult.Compilation.SyntaxTrees.Count().Equals(2));

            if (generationResult.GetDiagnostics().Any())
                _fixture.Logger.Info(generationResult.GetDiagnostics());

            _fixture.GenerationContext.GenerationResults.Add(generationResult);
            _fixture.GenerationContext.SuccessSyntaxTrees.Add(generationResult.Compilation.SyntaxTrees.Last());
        }

        _fixture.Logger.Info(_fixture.GenerationContext.GenerationResults.Count);
        _fixture.Logger.Info(_fixture.GenerationContext.SuccessSyntaxTrees.Count);

        var syntaxTrees = _fixture.GenerationContext.SuccessSyntaxTrees.DistinctBy(file => file.FilePath).ToList();
        var references = generationResult?.Compilation.References;

        var compilationOutputPath = Path.Combine(outputDir + "\\" + _fixture.GenerationContext.OutputAssemblyName);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var newCompilation = CSharpCompilation.Create(_fixture.GenerationContext.OutputAssemblyName, syntaxTrees,
            references, options);

        var arrayOfGenerators = ImmutableArray<BasicClassGenerator>.Empty;
        var arrayOfAdditionalTexts = ImmutableArray<AdditionalText>.Empty;
        var parseOptions = generationResult?.Compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var driver = CSharpGeneratorDriver.Create(arrayOfGenerators, arrayOfAdditionalTexts, parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(newCompilation, out var outputCompilation, out _);

        try
        {
            var result = outputCompilation.Emit(compilationOutputPath);

            Debug.Assert(result.Success.Equals(true));
            Debug.Assert(result.Diagnostics.IsEmpty);

            if (result.Diagnostics.Any())
                _fixture.Logger.Info(string.Join(Environment.NewLine,
                    result.Diagnostics.Select(diagnostic => diagnostic.Location.ToString())));
        }
        catch (IOException ex)
        {
            _fixture.Logger.Error(ex);
            throw;
        }
        finally
        {
            _fixture.Dispose();
        }
    }
}