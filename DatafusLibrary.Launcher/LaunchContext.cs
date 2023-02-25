using Cake.Common.Solution;
using Cake.Core;
using Cake.Frosting;

namespace DatafusLibrary.Launcher;

public class LaunchContext : FrostingContext
{
    public LaunchContext(ICakeContext context) : base(context)
    {
        var curDir = Directory.GetCurrentDirectory();
        var solutionDir = Directory.GetParent(curDir)?.Parent?.Parent;

        SolutionParserResult = context.ParseSolution(solutionDir + "\\DatafusLibrary.sln");

        ProjectsWithoutLauncher = SolutionParserResult.Projects
            .Where(project => !project.Path.FullPath.EndsWith("Launcher.csproj"))
            .ToList();
    }

    public SolutionParserResult SolutionParserResult { get; set; }

    public List<SolutionProject> ProjectsWithoutLauncher { get; set; }
}