using Cake.Frosting;

namespace DatafusLibrary.Launcher.Tasks;

[IsDependentOn(typeof(TestTask))]
public sealed class Default : FrostingTask
{
}