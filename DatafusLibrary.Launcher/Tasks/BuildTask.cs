using Cake.Common.Diagnostics;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Frosting;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("Build")]
[IsDependentOn(typeof(RestorePackagesTask))]
public sealed class BuildTask : AsyncFrostingTask<LaunchContext>
{
    public override Task RunAsync(LaunchContext context)
    {
        context.Information("Build started...");

        var buildSettings = new DotNetBuildSettings
        {
            NoRestore = true,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = "Debug"
        };

        foreach (var project in context.ProjectsWithoutLauncher)
        {
            context.Information($"Building at:  {project.Path.FullPath}");
            context.DotNetBuild(project.Path.FullPath, buildSettings);
        }

        context.Information("Build finished...");

        return Task.CompletedTask;
    }
}