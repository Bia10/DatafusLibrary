using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("Clean")]
[IsDependentOn(typeof(GetAssetsTask))]
public sealed class CleanTask : AsyncFrostingTask<LaunchContext>
{
    public override Task RunAsync(LaunchContext context)
    {
        context.Information("Clean started...");

        foreach (var project in context.ProjectsWithoutLauncher)
        {
            var projectFullPath = project.Path.FullPath;
            context.Information($"Project fullPath: {projectFullPath}");

            var replacementSegment = project.Path.Segments.Last();
            context.Information($"Replacement segment:: {replacementSegment}");

            var pathBin = projectFullPath.Replace(replacementSegment, "bin");
            var pathObj = projectFullPath.Replace(replacementSegment, "obj");

            context.CleanDirectories(pathBin);
            context.Information($"Cleaning bin at: {pathBin}");

            context.CleanDirectories(pathObj);
            context.Information($"Cleaning obj at: {pathObj}");
        }

        context.Information("Clean finished...");

        return Task.CompletedTask;
    }
}