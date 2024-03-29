﻿using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
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
    public readonly IEntityParser Parser = new EntityParser(new EntityDefinitionParser());
    public readonly GenerationContext GenerationContext;
    public ILogger? Logger;

    public GeneratorTestFixture()
    {
        GenerationContext = new GenerationContext(
            successSyntaxTrees: new List<SyntaxTree>(),
            failedSyntaxTrees: new List<SyntaxTree>(),
            generationResults: new List<GeneratorResult>(),
            inputTemplateName: "BasicClass.scriban",
            generatedSrcFileSuffix: ".generated.cs",
            generationOutputPath: "autogenerated",
            outputAssemblyName: "com.ankamagames.dofus.datacenter.dll");
    }

    public void Dispose()
    {
        if (Logger is null) return;

        Logger.Factory.Dispose();
        GC.SuppressFinalize(this);
    }

    public static ILogger MakeLogger(ITestOutputHelper testOutputHelper)
    {
        var logFactory = new LogFactory
        {
            ThrowExceptions = true,
            ThrowConfigExceptions = true,
            KeepVariablesOnReload = false,
            AutoShutdown = true,
            Configuration = null,
            GlobalThreshold = null,
            DefaultCultureInfo = CultureInfo.InvariantCulture
        };

        var configuration = new LoggingConfiguration();
        var testOutputTarget = new TestOutputTarget();

        testOutputTarget.Add(testOutputHelper, nameof(TemplateGeneratorTest));

        configuration.AddRuleForAllLevels(testOutputTarget, nameof(TemplateGeneratorTest));
        logFactory.Configuration = configuration;

        return logFactory.GetLogger(nameof(TemplateGeneratorTest));
    }
}

public class TemplateGeneratorTest : IClassFixture<GeneratorTestFixture>
{
    private readonly GeneratorTestFixture _fixture = new();

    public TemplateGeneratorTest(ITestOutputHelper testOutputHelper)
    {
        _fixture.Logger = GeneratorTestFixture.MakeLogger(testOutputHelper);
    }

    [Fact]
    public async Task GenerateFromTemplateTest()
    {
        ArgumentNullException.ThrowIfNull(_fixture.GenerationContext);
        ArgumentNullException.ThrowIfNull(_fixture.Logger);

        var allBasicClasses = await _fixture.Parser.GetAllBasicClassesFromDirAsync(_fixture.GenerationContext.JsonDataDirectoryPath);

        var templateString = TemplateLoader.LoadTemplate(_fixture.GenerationContext.InputTemplateName, typeof(TemplateLoader));

        GeneratorResult? generationResult = null;

        foreach (var basicClass in allBasicClasses)
        {
            if (basicClass.ClassName.Equals("LuaFormula", StringComparison.Ordinal))
                continue;

            var generatedSource = TemplateGenerator.Execute(templateString, basicClass);
            var generatedSourceFile = new GeneratedSourceFile(generatedSource, basicClass.ClassName + _fixture.GenerationContext.GeneratedSrcFileSuffix);
            var srcFilePath = Path.Combine(_fixture.GenerationContext.GenerationOutputDirectoryPath, generatedSourceFile.FileName);

            await generatedSourceFile.WriteToFile(srcFilePath);

            generationResult = GeneratorRunner.Run(generatedSourceFile.SourceCode, new BasicClassGenerator());

            Debug.Assert(generationResult.Compilation.SyntaxTrees.Count().Equals(2));

            if (generationResult.GetDiagnostics().Any())
                _fixture.Logger.Info(generationResult.GetDiagnostics());

            _fixture.GenerationContext.GenerationResults.Add(generationResult);
            _fixture.GenerationContext.SuccessSyntaxTrees.Add(generationResult.Compilation.SyntaxTrees.Last());
        }

        _fixture.Logger.Info("GenerationResults count: " + _fixture.GenerationContext.GenerationResults.Count);
        _fixture.Logger.Info("SuccessSyntaxTrees count:  " + _fixture.GenerationContext.SuccessSyntaxTrees.Count);

        var syntaxTrees = _fixture.GenerationContext.SuccessSyntaxTrees.DistinctBy(file => file.FilePath).ToList();
        var metadataReferences = generationResult?.Compilation.References;
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var newCompilation = CSharpCompilation.Create(_fixture.GenerationContext.OutputAssemblyName, syntaxTrees, metadataReferences, options);
        var arrayOfGenerators = ImmutableArray<BasicClassGenerator>.Empty;
        var arrayOfAdditionalTexts = ImmutableArray<AdditionalText>.Empty;
        var parseOptions = generationResult?.Compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var driver = CSharpGeneratorDriver.Create(arrayOfGenerators, arrayOfAdditionalTexts, parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(newCompilation, out var outputCompilation, out _);

        try
        {
            _fixture.Logger.Info($"Outputing assembly to path: {_fixture.GenerationContext.GenerationOutputAssemblyPath}");

            var result = outputCompilation.Emit(_fixture.GenerationContext.GenerationOutputAssemblyPath);

            if (result.Diagnostics.Any())
                _fixture.Logger.Info(string.Join(Environment.NewLine,
                    result.Diagnostics.Select(diagnostic => diagnostic.Location.ToString())));

            Debug.Assert(result.Success.Equals(true));
            Debug.Assert(result.Diagnostics.IsEmpty.Equals(true));
            Debug.Assert(File.Exists(_fixture.GenerationContext.GenerationOutputAssemblyPath).Equals(true));
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