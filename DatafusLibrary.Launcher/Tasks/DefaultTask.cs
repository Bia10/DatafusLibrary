using Cake.Frosting;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("Default")]
[IsDependentOn(typeof(TestTask))]
public sealed class DefaultTask : AsyncFrostingTask
{
}