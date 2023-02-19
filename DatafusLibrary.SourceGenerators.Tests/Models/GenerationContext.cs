using Microsoft.CodeAnalysis;

namespace DatafusLibrary.SourceGenerators.Tests.Models;

public class GenerationContext
{
    public GenerationContext(
        string outputAssemblyName, 
        string generationOutputPath, 
        string inputTemplateName,
        string jsonDataDirectoryPath,
        string generatedSrcFileSuffix,
        List<SyntaxTree> successSyntaxTrees,
        List<SyntaxTree> failedSyntaxTrees,
        List<GeneratorResult> generationResults)
    {
        OutputAssemblyName = outputAssemblyName;
        GenerationOutputPath = generationOutputPath;
        InputTemplateName = inputTemplateName;
        JsonDataDirectoryPath = jsonDataDirectoryPath;
        GeneratedSrcFileSuffix = generatedSrcFileSuffix;
        SuccessSyntaxTrees = successSyntaxTrees;
        FailedSyntaxTrees = failedSyntaxTrees;
        GenerationResults = generationResults;
    }

    public string GenerationOutputPath { get; set; }
    public string OutputAssemblyName { get; set; }
    public string InputTemplateName { get; set; }
    public string JsonDataDirectoryPath { get; set; }
    public string GeneratedSrcFileSuffix { get; set; }

    public List<SyntaxTree> SuccessSyntaxTrees { get; set; }
    public List<SyntaxTree> FailedSyntaxTrees { get; set; }
    public List<GeneratorResult> GenerationResults { get; set; }

    public static string SetInputDataPath()
    {
        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            return desktopPath + "\\Dofus2Botting\\data\\entities_json";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

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