using System.Text;

namespace DatafusLibrary.Core.IO;

public static class FileReader
{
    private const int DefaultBufferSize = 4096;
    private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

    public static Task<string[]> ReadAllLinesAsync(string path, string? terminatingLine = null)
    {
        return ReadAllLinesAsync(path, Encoding.UTF8, terminatingLine);
    }

    private static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding,
        string? terminatingLine = null)
    {
        try
        {
            List<string> lines = new();

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                DefaultBufferSize, DefaultOptions);
            using (var reader = new StreamReader(stream, encoding))
            {
                if (terminatingLine is null)
                    while (await reader.ReadLineAsync() is { } line)
                        lines.Add(line);
                else
                    while (await reader.ReadLineAsync() is { } line &&
                           !line.Equals(terminatingLine, StringComparison.Ordinal))
                        lines.Add(line);
            }

            return lines.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}