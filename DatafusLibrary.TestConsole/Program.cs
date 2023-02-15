namespace DatafusLibrary.TestConsole;

internal static class Program
{
    private static async Task Main()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory);
        var solutionDirectory = projectDirectory?.Parent?.Parent?.Parent;
        var path = solutionDirectory + @"\DatafusLibrary.TestConsole\MockData\Areas.json";
    }
}