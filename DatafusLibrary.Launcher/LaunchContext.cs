using Cake.Common.Diagnostics;
using Cake.Common.Solution;
using Cake.Core;
using Cake.Frosting;

namespace DatafusLibrary.Launcher;

public sealed class LaunchContext : FrostingContext
{
    public LaunchContext(ICakeContext context) : base(context)
    {
        context.Information("LaunchContext build started...");

        LocalPathProvider = new PathProvider(context);

        SolutionParserResult = context.ParseSolution(LocalPathProvider.SolutionPath);

        foreach (var project in SolutionParserResult.Projects)
            context.Information($"Found project: {project.Name} at path: {project.Path.FullPath}");

        ProjectsToProcess = SolutionParserResult.Projects
            .Where(project => !project.Name.Equals("DatafusLibrary.Launcher", StringComparison.Ordinal)).ToList();

        LocalPathProvider.LoadTestProjectFullPath(ProjectsToProcess);

        context.Information("LaunchContext build finished...");
    }

    public PathProvider LocalPathProvider { get; }

    public SolutionParserResult SolutionParserResult { get; }

    public List<SolutionProject> ProjectsToProcess { get; }
}