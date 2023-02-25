using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("Clean")]
public sealed class CleanTask : AsyncFrostingTask<LaunchContext>
{
    public override Task RunAsync(LaunchContext context)
    {
        context.Information("Clean started...");

        foreach (var project in context.ProjectsWithoutLauncher)
        {
            var pathBin = project.Path.FullPath.Replace(project.Path.Segments[7], "bin");
            var pathObj = project.Path.FullPath.Replace(project.Path.Segments[7], "obj");

            context.CleanDirectories(pathBin);
            context.Information($"Cleaning bin at:  {pathBin}");

            context.CleanDirectories(pathObj);
            context.Information($"Cleaning obj at:  {pathObj}");
        }

        context.Information("Clean finished...");

        return Task.CompletedTask;
    }
}