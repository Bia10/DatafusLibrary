using System.Text;

namespace DatafusLibrary.SourceGenerators.Tests;

public class GeneratedSourceFile
{
    public GeneratedSourceFile(string sourceCode, string fileName)
    {
        SourceCode = sourceCode;
        FileName = fileName;
    }

    public string SourceCode { get; set; }
    public string FileName { get; set; }

    public async Task WriteToFile(string filePath)
    {
        await File.WriteAllTextAsync(filePath, SourceCode, Encoding.UTF8);
    }
}