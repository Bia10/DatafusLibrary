using System.Text;

namespace DatafusLibrary.Core.IO;

public static class FileReader
{
    private const int DefaultBufferSize = 4096;
    private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

    public static Task<string[]> ReadAllLinesAsync(string path)
    {
        return ReadAllLinesAsync(path, Encoding.UTF8);
    }

    private static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding)
    {
        try
        {
            List<string> lines = new();

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                DefaultBufferSize, DefaultOptions);
            using (var reader = new StreamReader(stream, encoding))
            {
                while (await reader.ReadLineAsync() is { } line)
                {
                    lines.Add(line);
                }
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