namespace DatafusLibrary.SourceGenerators;

public class GeneratedSourceFile
{
    public GeneratedSourceFile(string sourceCode, string fileName)
    {
        SourceCode = sourceCode;
        FileName = fileName;
    }

    public string SourceCode { get; set; }
    public string FileName { get; set; }
}