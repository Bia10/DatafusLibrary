using System.Runtime.Loader;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;
using DatafusLibrary.TestRunner;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("Test")]
[IsDependentOn(typeof(BuildTask))]
public sealed class TestTask : AsyncFrostingTask<LaunchContext>
{
    public override Task RunAsync(LaunchContext context)
    {
        context.Information("Test started...");

        if (context.FileExists(context.TestProjectAssemblyPath))
        {
            var assemblyFiles =
                Directory.EnumerateFiles(context.TestProjectOutputPath, "*.dll", SearchOption.AllDirectories);

            foreach (var assemblyFile in assemblyFiles)
                try
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
                }
                catch (Exception ex)
                {
                    context.Error(ex);
                }

            var xUnitTestRunner = new XUnitTestRunner(context.TestProjectAssemblyPath);

            xUnitTestRunner.DiscoverTests();

            while (!xUnitTestRunner.DiscoveryContext.IsFinished)
            {
                // nop
            }

            if (xUnitTestRunner.DiscoveryContext.FoundTests is not null &&
                xUnitTestRunner.DiscoveryContext.FoundTests.Any())
            {
                context.Information($"Found tests: {xUnitTestRunner.DiscoveryContext.FoundTests.Count}");

                var orderedTests = xUnitTestRunner.OrderTests(xUnitTestRunner.DiscoveryContext.FoundTests);

                context.Information($"Running test: {orderedTests.First().TestMethod.Method.Name}");

                xUnitTestRunner.RunTests(orderedTests);

                while (!xUnitTestRunner.ExecutionContext.IsFinished)
                {
                    // nop
                }

                if (xUnitTestRunner.ExecutionContext.TestsFailed is not null)
                    context.Information($"Failed test: {xUnitTestRunner.ExecutionContext.TestsFailed.Count}");
                if (xUnitTestRunner.ExecutionContext.TestsPassed is not null)
                    context.Information($"Success test: {xUnitTestRunner.ExecutionContext.TestsPassed.Count}");
            }

            xUnitTestRunner.Dispose();
        }

        context.Information("Test finished...");

        return Task.CompletedTask;
    }
}