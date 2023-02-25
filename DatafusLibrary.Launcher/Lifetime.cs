using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Frosting;

namespace DatafusLibrary.Launcher;

public sealed class Lifetime : FrostingLifetime<LaunchContext>
{
    public override void Setup(LaunchContext context, ISetupContext setupContext)
    {
        context.Information("Setup starting...");

        context.Information("Setup finished...");
    }

    public override void Teardown(LaunchContext context, ITeardownContext teardownContext)
    {
        context.Information("Teardown starting...");

        context.Information("Teardown finished...");
    }
}