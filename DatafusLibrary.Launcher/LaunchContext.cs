using Cake.Common.Diagnostics;
using Cake.Common.Solution;
using Cake.Core;
using Cake.Frosting;

namespace DatafusLibrary.Launcher;

public sealed class LaunchContext : FrostingContext
{
    public LaunchContext(ICakeContext context) : base(context)
    {
        LocalPathProvider = new PathProvider(context);

        context.Information("Building launch context ...");

        SolutionParserResult = context.ParseSolution(LocalPathProvider.SolutionPath);

        foreach (var project in SolutionParserResult.Projects)
        {
            context.Information($"Project: {project.Name}");
            context.Information($"Project path: {project.Path.FullPath}");
        }

        ProjectsWithoutLauncher = SolutionParserResult.Projects
            .Where(project => !project.Name.Equals("DatafusLibrary.Launcher", StringComparison.Ordinal))
            .ToList();

        LocalPathProvider.ToTestProjectFullPath(ProjectsWithoutLauncher);
        LocalPathProvider.ToTestProjectOutput();
        LocalPathProvider.ToTestAssemblyPath();
    }

    public PathProvider LocalPathProvider { get; }

    public SolutionParserResult SolutionParserResult { get; }

    public List<SolutionProject> ProjectsWithoutLauncher { get; }
}