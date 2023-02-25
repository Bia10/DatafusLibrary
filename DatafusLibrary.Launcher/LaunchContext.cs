using Cake.Common.Solution;
using Cake.Core;
using Cake.Frosting;

namespace DatafusLibrary.Launcher;

public sealed class LaunchContext : FrostingContext
{
    public LaunchContext(ICakeContext context) : base(context)
    {
        var curDir = Directory.GetCurrentDirectory();
        var solutionDir = Directory.GetParent(curDir)?.Parent?.Parent;
        var solutionFilePath = solutionDir + "\\DatafusLibrary.sln";

        if (OperatingSystem.IsLinux()) 
            solutionFilePath = solutionFilePath.Replace('\\', '/');

        SolutionParserResult = context.ParseSolution(solutionFilePath);

        ProjectsWithoutLauncher = SolutionParserResult.Projects
            .Where(project => !project.Name.Equals("DatafusLibrary.Launcher", StringComparison.Ordinal))
            .ToList();

        var testsProject = SolutionParserResult.Projects
            .FirstOrDefault(project => project.Name
                .Equals("DatafusLibrary.SourceGenerators.Tests", StringComparison.Ordinal));

        if (testsProject is null)
            throw new NullReferenceException(nameof(testsProject));

        var testProjectOutputPath = testsProject.Path.FullPath.Replace(
            testsProject.Path.Segments.Last(),
            "\\bin\\Debug\\net7.0\\");

        if (OperatingSystem.IsLinux()) 
            testProjectOutputPath = testProjectOutputPath.Replace('\\', '/');

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