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

        SolutionParserResult = context.ParseSolution(solutionDir + "\\DatafusLibrary.sln");

        ProjectsWithoutLauncher = SolutionParserResult.Projects
            .Where(project => !project.Path.FullPath.EndsWith("Launcher.csproj"))
            .ToList();

        var testsProject = SolutionParserResult.Projects
            .FirstOrDefault(project => project.Name
                .Equals("DatafusLibrary.SourceGenerators.Tests", StringComparison.Ordinal));

        if (testsProject is null)
            throw new NullReferenceException(nameof(testsProject));

        TestProjectOutputPath = testsProject.Path.FullPath.Replace(
            testsProject.Path.Segments.Last(),
            "\\bin\\Debug\\net7.0\\");

        TestProjectAssemblyPath = testsProject.Path.FullPath.Replace(
            testsProject.Path.Segments.Last(),
            "\\bin\\Debug\\net7.0\\DatafusLibrary.SourceGenerators.Tests.dll");
    }

    public SolutionParserResult SolutionParserResult { get; }

    public List<SolutionProject> ProjectsWithoutLauncher { get; }

    public string TestProjectOutputPath { get; }

    public string TestProjectAssemblyPath { get; }
}