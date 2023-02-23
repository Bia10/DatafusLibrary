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

public class TemplateGeneratorTest : IDisposable
{
    private readonly GenerationContext _generationContext;
    private readonly ILogger _logger;

    public TemplateGeneratorTest(ITestOutputHelper iTestOutputHelper)
    {
        var logFactory = new LogFactory();
        logFactory.ThrowExceptions = true;
        var configuration = new LoggingConfiguration();
        var testOutputTarget = new TestOutputTarget();

        testOutputTarget.Add(iTestOutputHelper, nameof(TemplateGeneratorTest));
        configuration.AddRuleForAllLevels(testOutputTarget, nameof(TemplateGeneratorTest));
        logFactory.Configuration = configuration;

        _logger = logFactory.GetLogger(nameof(TemplateGeneratorTest));
        _logger.Info("TemplateGenerator Test Init!");

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _logger.Factory.Dispose();
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

            if (generationResult.GetDiagnostics().Any())
                _logger.Info(generationResult.GetDiagnostics());

            _generationContext.GenerationResults.Add(generationResult);
            _generationContext.SuccessSyntaxTrees.Add(generationResult.Compilation.SyntaxTrees.Last());
        }

        _logger.Info(_generationContext.GenerationResults.Count);
        _logger.Info(_generationContext.SuccessSyntaxTrees.Count);

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

        try
        {
            var result = outputCompilation.Emit(compilationOutputPath);

            Debug.Assert(result.Success.Equals(true));
            Debug.Assert(result.Diagnostics.IsEmpty);

            if (result.Diagnostics.Any())
                _logger.Info(string.Join(Environment.NewLine,
                    result.Diagnostics.Select(diagnostic => diagnostic.Location.ToString())));
        }
        catch (IOException ex)
        {
            _logger.Error(ex);
            throw;
        }
        finally
        {
            Dispose();
        }
    }
}