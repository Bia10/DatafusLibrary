using Cake.Frosting;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("Default")]
[IsDependentOn(typeof(PackTask))]
public sealed class DefaultTask : AsyncFrostingTask
{
}