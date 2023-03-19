using System.Text;

namespace DatafusLibrary.Core.IO;

internal static class FileWriter
{
    private const int DefaultBufferSize = 4096;
    private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

    internal static Task WriteAllLinesAsync(string path, IEnumerable<string> lines)
    {
        return WriteAllLinesAsync(path, Encoding.UTF8, lines);
    }

    private static async Task WriteAllLinesAsync(string path, Encoding encoding, IEnumerable<string> lines)
    {
        try
        {
            await using var stream = new FileStream(path,
                FileMode.Open,
                FileAccess.Write,
                FileShare.Write,
                DefaultBufferSize,
                DefaultOptions);

            await using var writer = new StreamWriter(stream, encoding);

            foreach (var line in lines)
                await writer.WriteLineAsync(line);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}