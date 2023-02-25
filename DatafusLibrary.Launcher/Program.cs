using Cake.Frosting;
using Microsoft.Extensions.DependencyInjection;

namespace DatafusLibrary.Launcher;

public class Program : IFrostingStartup
{
    public void Configure(IServiceCollection services)
    {
        services.UseWorkingDirectory("..");
        services.UseContext<LaunchContext>();
        services.UseLifetime<Lifetime>();
    }

    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseStartup<Program>()
            .Run(args);
    }
}