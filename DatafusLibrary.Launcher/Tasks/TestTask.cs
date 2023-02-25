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

        var testsProject = context.SolutionParserResult.Projects.FirstOrDefault(project =>
            project.Name.Equals("DatafusLibrary.SourceGenerators.Tests", StringComparison.Ordinal));

        if (testsProject is null) 
            throw new NullReferenceException(nameof(testsProject));

        var pathToOutput = testsProject.Path.FullPath.Replace(
            testsProject.Path.Segments.Last(),
            "\\bin\\Debug\\net7.0\\");
        var pathToTestsAssembly = testsProject.Path.FullPath.Replace(
            testsProject.Path.Segments.Last(),
            "\\bin\\Debug\\net7.0\\DatafusLibrary.SourceGenerators.Tests.dll");

        if (context.FileExists(pathToTestsAssembly))
        {
            LoadAllRequiredAssemblies(pathToOutput);

            var xUnitTestRunner = new XUnitTestRunner(pathToTestsAssembly);

            xUnitTestRunner.DiscoverTests();

            while (!xUnitTestRunner.DiscoveryContext.IsFinished)
            {
                // nop
            }

            if (xUnitTestRunner.DiscoveryContext.FoundTests is not null &&
                xUnitTestRunner.DiscoveryContext.FoundTests.Any())
                context.Information($"Found tests: {xUnitTestRunner.DiscoveryContext.FoundTests.Count}");

            if (xUnitTestRunner.DiscoveryContext.FoundTests is not null)
            {
                var orderedTests = xUnitTestRunner.OrderTests(xUnitTestRunner.DiscoveryContext.FoundTests);

                context.Information($"Running test: {orderedTests.First().DisplayName}");

                xUnitTestRunner.RunTests(orderedTests);

                while (!xUnitTestRunner.ExecutionContext.IsFinished)
                {
                    // nop
                }

                if (xUnitTestRunner.ExecutionContext.TestsFailed is not null)
                    context.Information($"Failed test: {xUnitTestRunner.ExecutionContext.TestsFailed.Count}");
                if (xUnitTestRunner.ExecutionContext.TestsPassed is not null)
                    context.Information($"Succes test: {xUnitTestRunner.ExecutionContext.TestsPassed.Count}");
            }
        }

        context.Information("Test finished...");

        return Task.CompletedTask;
    }

    public static void LoadAllRequiredAssemblies(string assemblyDir)
    {
        var assemblyFiles = Directory.EnumerateFiles(assemblyDir, "*.dll", SearchOption.AllDirectories);

        foreach (var assemblyFile in assemblyFiles)
            try
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
            }
            catch (Exception)
            {
                // _logger.Error(ex);
            }
    }
}