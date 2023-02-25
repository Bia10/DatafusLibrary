using Cake.Common.Diagnostics;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("Restore")]
[IsDependentOn(typeof(CleanTask))]
public sealed class RestorePackagesTask : AsyncFrostingTask<LaunchContext>
{
    public override Task RunAsync(LaunchContext context)
    {
        context.Information("Restore started...");

        foreach (var project in context.SolutionParserResult.Projects)
        {
            context.Information($"Restoring packages at:  {project.Path.FullPath}");
            context.DotNetRestore(project.Path.FullPath);
        }

        context.Information("Restore finished...");

        return Task.CompletedTask;
    }
}