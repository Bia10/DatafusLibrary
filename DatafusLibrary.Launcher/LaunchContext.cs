using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Core;
using Cake.Frosting;

namespace DatafusLibrary.Launcher;

public sealed class LaunchContext : FrostingContext
{
    public LaunchContext(ICakeContext context) : base(context)
    {
        context.Information("Building launch context ...");

        var curDir = Directory.GetCurrentDirectory();
        var solutionDir = Directory.GetParent(curDir)?.Parent?.Parent;
        var solutionFilePath = solutionDir + "\\DatafusLibrary.sln";

        if (OperatingSystem.IsLinux())
            solutionFilePath = curDir + "/DatafusLibrary.sln";;

        if (!context.FileExists(solutionFilePath))
            context.Error($"File at path: {solutionFilePath} not found!");

        SolutionParserResult = context.ParseSolution(solutionFilePath);

        ProjectsWithoutLauncher = SolutionParserResult.Projects
            .Where(project => !project.Name.Equals("DatafusLibrary.Launcher", StringComparison.Ordinal))
            .ToList();

        var testsProject = SolutionParserResult.Projects
            .FirstOrDefault(project => project.Name
                .Equals("DatafusLibrary.SourceGenerators.Tests", StringComparison.Ordinal));

        if (testsProject is null)
            throw new NullReferenceException(nameof(testsProject));

        context.Information($"Test project path: {testsProject.Path.FullPath}");

        var testProjectOutputPath = testsProject.Path.FullPath.Replace(
            testsProject.Path.Segments.Last(),
            "\\bin\\Debug\\net7.0\\");

        if (OperatingSystem.IsLinux())
            testProjectOutputPath = testProjectOutputPath.Replace('\\', '/');

        context.Information($"testProjectOutputPath: {testProjectOutputPath}");

        TestProjectOutputPath = testProjectOutputPath;

        var testProjectAssemblyPath = testsProject.Path.FullPath.Replace(
            testsProject.Path.Segments.Last(),
            "\\bin\\Debug\\net7.0\\DatafusLibrary.SourceGenerators.Tests.dll");

        if (OperatingSystem.IsLinux())
            testProjectAssemblyPath = testProjectAssemblyPath.Replace('\\', '/');

        TestProjectAssemblyPath = testProjectAssemblyPath;
    }

    public SolutionParserResult SolutionParserResult { get; }

    public List<SolutionProject> ProjectsWithoutLauncher { get; }

    public string TestProjectOutputPath { get; }

    public string TestProjectAssemblyPath { get; }
}