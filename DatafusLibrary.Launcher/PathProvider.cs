using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Path = System.IO.Path;

namespace DatafusLibrary.Launcher;

public class PathProvider : FrostingContext
{
    private static string? _currentDirectory;
    private static ICakeContext? _context;
    private static FilePath? _testProjectPath;

    public readonly FilePath SolutionPath;
    public readonly FilePath TempPath;
    public FilePath? TestProjectAssemblyPath;
    public FilePath? TestProjectOutputPath;

    public PathProvider(ICakeContext? context) : base(context)
    {
        _context = context;
        _currentDirectory = Directory.GetCurrentDirectory();

        SolutionPath = ToSolution();
        TempPath = ToTemp();
    }

    private static string ToSolution()
    {
        var solutionDir = Directory.GetParent(_currentDirectory!)?.Parent?.Parent;
        var solutionFilePath = solutionDir + "\\DatafusLibrary.sln";

        if (OperatingSystem.IsLinux())
            solutionFilePath = _currentDirectory + "/DatafusLibrary/DatafusLibrary.sln";

        if (!_context.FileExists(solutionFilePath))
            _context.Error($"File at path: {solutionFilePath} not found!");

        return solutionFilePath;
    }

    private static string ToTemp()
    {
        var tempPath = Path.GetTempPath();

        if (OperatingSystem.IsLinux())
            tempPath = "/home/runner/work/_temp/";

        if (!_context.DirectoryExists(tempPath))
            _context.Error($"Dir at path: {tempPath} not found!");

        return tempPath;
    }

    public void ToTestProjectFullPath(IEnumerable<SolutionProject> projects)
    {
        var testsProject = projects.FirstOrDefault(project => project.Name
            .Equals("DatafusLibrary.SourceGenerators.Tests", StringComparison.Ordinal));

        if (testsProject is null)
            throw new NullReferenceException(nameof(testsProject));

        if (testsProject.Path is null)
            throw new NullReferenceException(nameof(testsProject.Path));

        if (!_context.FileExists(testsProject.Path.FullPath))
            _context.Error($"File at path: {testsProject.Path.FullPath} not found!");

        _context.Information($"Test project path: {testsProject.Path.FullPath}");

        _testProjectPath = testsProject.Path;
    }

    public void ToTestProjectOutput()
    {
        var outputPath = OperatingSystem.IsLinux()
            ? "\\bin\\Debug\\net7.0\\"
            : "/bin/Debug/net7.0/";

        var testProjectOutputPath = _testProjectPath?.FullPath
            .Replace(_testProjectPath.Segments.Last(), outputPath);

        _context.Information($"Test project output path: {testProjectOutputPath}");

        TestProjectOutputPath = testProjectOutputPath;
    }

    public void ToTestAssemblyPath()
    {
        var outputPath = OperatingSystem.IsLinux()
            ? "\\bin\\Debug\\net7.0\\"
            : "/bin/Debug/net7.0/";

        var testProjectAssemblyPath = _testProjectPath?.FullPath.Replace(
            _testProjectPath.Segments.Last(),
            outputPath + "DatafusLibrary.SourceGenerators.Tests.dll");

        _context.Information($"Test project assembly path: {testProjectAssemblyPath}");

        TestProjectAssemblyPath = testProjectAssemblyPath;
    }
}