using System.Text;

namespace DatafusLibrary.Core.IO;

public static class FileReader
{
    private const int DefaultBufferSize = 4096;
    private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

    public static async Task<string[]> ReadAllLinesAsync(string path, string? terminatingLine = null)
    {
        return await ReadAllLinesAsync(path, Encoding.UTF8, terminatingLine);
    }

    public static async Task<string> ReadAllAsync(string path, string? terminatingLine = null)
    {
        var results = await ReadAllLinesAsync(path, Encoding.UTF8, terminatingLine);

        return string.Join(string.Empty, results);
    }

    private static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, string? terminatingLine = null)
    {
        try
        {
            var lines = new List<string>();

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions);

            using var reader = new StreamReader(stream, encoding);

            if (terminatingLine is null)
                while (await reader.ReadLineAsync() is { } line)
                    lines.Add(line);
            else
                while (await reader.ReadLineAsync() is { } line &&
                       !line.Equals(terminatingLine, StringComparison.Ordinal))
                    lines.Add(line);

            return lines.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}