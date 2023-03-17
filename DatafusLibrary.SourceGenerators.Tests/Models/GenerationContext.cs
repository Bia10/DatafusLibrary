using Microsoft.CodeAnalysis;

namespace DatafusLibrary.SourceGenerators.Tests.Models;

public class GenerationContext
{
    internal GenerationContext(
        string outputAssemblyName,
        string generationOutputPath,
        string inputTemplateName,
        string generatedSrcFileSuffix,
        List<SyntaxTree> successSyntaxTrees,
        List<SyntaxTree> failedSyntaxTrees,
        List<GeneratorResult> generationResults)
    {
        OutputAssemblyName = outputAssemblyName;
        GenerationOutputPath = generationOutputPath;
        InputTemplateName = inputTemplateName;
        GeneratedSrcFileSuffix = generatedSrcFileSuffix;
        SuccessSyntaxTrees = successSyntaxTrees;
        FailedSyntaxTrees = failedSyntaxTrees;
        GenerationResults = generationResults;

        JsonDataDirectoryPath = OperatingSystem.IsLinux()
            ? "/home/runner/work/_temp/datafusRelease/data/entities_json"
            : Path.GetTempPath() + @"\datafusRelease\data\entities_json";

        GenerationOutputDirectoryPath = CreateOutputDir(JsonDataDirectoryPath, GenerationOutputPath);
        GenerationOutputAssemblyPath = Path.Combine(GenerationOutputDirectoryPath +
                                                    (OperatingSystem.IsLinux() ? "/" : "\\") + OutputAssemblyName);
    }

    public string GenerationOutputPath { get; }
    public string OutputAssemblyName { get; }
    public string InputTemplateName { get; }
    public string JsonDataDirectoryPath { get; init; }
    public string GeneratedSrcFileSuffix { get; }
    public string GenerationOutputDirectoryPath { get; }
    public string GenerationOutputAssemblyPath { get; }
    public List<SyntaxTree> SuccessSyntaxTrees { get; }
    public List<SyntaxTree> FailedSyntaxTrees { get; }
    public List<GeneratorResult> GenerationResults { get; }

    public static string CreateOutputDir(string inputDataPath, string outputPath)
    {
        try
        {
            var outputDir = Path.Combine(inputDataPath, outputPath);

            if (Directory.Exists(outputDir))
                return outputDir;

            var dirInfo = Directory.CreateDirectory(outputDir);
            return dirInfo.FullName;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}