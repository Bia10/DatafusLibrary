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

        var testProjectAssemblyPath = context.LocalPathProvider.TestProjectAssemblyPath;

        if (testProjectAssemblyPath is null)
            throw new NullReferenceException(nameof(testProjectAssemblyPath));

        if (context.FileExists(testProjectAssemblyPath))
        {
            var testProjectOutputPath = context.LocalPathProvider.TestProjectOutputPath?.FullPath;

            if (string.IsNullOrEmpty(testProjectOutputPath))
                throw new NullReferenceException(nameof(testProjectOutputPath));

            var assemblyFiles = Directory.EnumerateFiles(testProjectOutputPath, "*.dll", SearchOption.AllDirectories);

            foreach (var assemblyFile in assemblyFiles)
                try
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
                }
                catch (Exception ex)
                {
                    context.Error(ex);
                }

            var xUnitTestRunner = new XUnitTestRunner(testProjectAssemblyPath.FullPath);

            xUnitTestRunner.DiscoverTests();

            while (!xUnitTestRunner.DiscoveryContext.IsFinished)
            {
                if (xUnitTestRunner.DiagnosticContext.Errors.Any())
                    context.Error(xUnitTestRunner.DiagnosticContext.GetErrors());
                if (xUnitTestRunner.DiagnosticContext.Diagnostics.Any())
                    context.Warning(xUnitTestRunner.DiagnosticContext.GetDiagnostics());
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
                    if (xUnitTestRunner.DiagnosticContext.Errors.Any())
                        context.Error(xUnitTestRunner.DiagnosticContext.GetErrors());
                    if (xUnitTestRunner.DiagnosticContext.Diagnostics.Any())
                        context.Warning(xUnitTestRunner.DiagnosticContext.GetDiagnostics());
                    if (xUnitTestRunner.ExecutionContext.TestOutputs.Any())
                        context.Information(xUnitTestRunner.ExecutionContext.GetTestOutput());
                }

                foreach (var passedTest in xUnitTestRunner.ExecutionContext.TestsPassed)
                    context.Information(
                        $"Passing test: {passedTest.TestMethod.Method.Name} execution time: {passedTest.ExecutionTime}");

                if (xUnitTestRunner.ExecutionContext.TestsFailed.Any())
                {
                    context.Warning($"Failed tests: {xUnitTestRunner.ExecutionContext.TestsFailed.Count}");

                    return Task.FromException(new InvalidOperationException("Tests failed..."));
                }
            }

            xUnitTestRunner.Dispose();
        }

        context.Information("Test finished...");

        return Task.CompletedTask;
    }
}