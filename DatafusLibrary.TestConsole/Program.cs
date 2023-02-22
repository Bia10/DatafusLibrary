using DatafusLibrary.SourceGenerators.Tests.Generation;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.TestConsole;

public static class Program
{
    private static readonly object ConsoleLock = new();
    private static readonly ManualResetEvent Finished = new(false);
    private static IEnumerable<ITestCase> _testCasesFound = new List<ITestCase>();
    private static bool _discoveryDone;

    private static Task Main()
    {
        var assemblyLocation = typeof(TemplateGeneratorTest).Assembly.Location;

        var testMessageSink = new TestMessageSink();

        testMessageSink.Discovery.TestCaseDiscoveryMessageEvent += DiscoveryEventSink_TestCaseDiscoveryMessageEvent;
        testMessageSink.Discovery.DiscoveryCompleteMessageEvent += DiscoveryEventSink_DiscoveryCompleteMessageEvent;

        testMessageSink.Diagnostics.DiagnosticMessageEvent += MessagesEventSink_DiagnosticMessageEvent;
        testMessageSink.Diagnostics.ErrorMessageEvent += MessagesEventSink_ErrorMessageEvent;

        testMessageSink.Execution.TestAssemblyStartingEvent += ExecutionEvenSink_TestAssemblyStartingEvent;
        testMessageSink.Execution.TestAssemblyFinishedEvent += ExecutionEvenSink_TestAssemblyFinishedEvent;
        testMessageSink.Execution.TestPassedEvent += ExecutionEvenSink_TestPassedEvent;
        testMessageSink.Execution.TestFailedEvent += ExecutionEvenSink_TestFailedEvent;
        testMessageSink.Execution.TestOutputEvent += ExecutionEventSink_TestOutputEvent;
        
        testMessageSink.Runner.TestExecutionSummaryEvent += ExecutionEventSink_TestSummaryEvent;
        
        var xUnit = new XunitFrontController(
            AppDomainSupport.IfAvailable,
            assemblyLocation,
            null,
            false,
            null,
            null,
            testMessageSink);

        var assemblyOptions = new TestAssemblyConfiguration
        {
            AppDomain = AppDomainSupport.IfAvailable,
            DiagnosticMessages = true,
            InternalDiagnosticMessages = true,
            MethodDisplay = TestMethodDisplay.ClassAndMethod
        };

        Console.WriteLine($"TestFrameworkDisplayName: {xUnit.TestFrameworkDisplayName}" +
                          $"\n TargetFramework: {xUnit.TargetFramework}" +
                          $"\n CanUseAppDomains: {xUnit.CanUseAppDomains}");

        var discoveryOptions = TestFrameworkOptions.ForDiscovery(assemblyOptions);
        var executionOptions = TestFrameworkOptions.ForExecution(assemblyOptions);

        xUnit.Find(true, testMessageSink, discoveryOptions);

        while (!_discoveryDone)
        {
        }

        var testCaseOrderer = new TestCaseOrderer(testMessageSink);

        _testCasesFound = testCaseOrderer.OrderTestCases(_testCasesFound);

        xUnit.RunTests(_testCasesFound, testMessageSink, executionOptions);

        try
        {
            Finished.WaitOne();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            Finished.Dispose();
        }

        return Task.CompletedTask;
    }

    private static void ExecutionEventSink_TestSummaryEvent(MessageHandlerArgs<ITestExecutionSummary> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"Execution summary event, time elapsed: {args.Message.ElapsedClockTime}");

            foreach (var summaryKvp in args.Message.Summaries)
            {
                Console.WriteLine($"Index: {summaryKvp.Key} Summary: {summaryKvp.Value}");
            }
        }
    }

    private static void DiscoveryEventSink_TestCaseDiscoveryMessageEvent(
        MessageHandlerArgs<ITestCaseDiscoveryMessage> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine(
                $"TestDiscovery event, \n found test: {args.Message.TestCase.TestMethod.Method.Name} \n in class: {args.Message.TestClass.Class.Name}");

            _testCasesFound = _testCasesFound.Append(args.Message.TestCase);
        }
    }

    private static void DiscoveryEventSink_DiscoveryCompleteMessageEvent(
        MessageHandlerArgs<IDiscoveryCompleteMessage> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"DiscoveryComplete event at: {DateTime.Now}");
            _discoveryDone = true;
        }
    }

    private static void MessagesEventSink_ErrorMessageEvent(MessageHandlerArgs<IErrorMessage> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"ErrorMessage event: {args.Message.Messages}");
        }
    }

    private static void MessagesEventSink_DiagnosticMessageEvent(MessageHandlerArgs<IDiagnosticMessage> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"DiagnosticMessage event: {args.Message.Message}");
        }
    }

    private static void ExecutionEventSink_TestOutputEvent(MessageHandlerArgs<ITestOutput> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"TestOutput: {args.Message.Output}");
        }
    }

    private static void ExecutionEvenSink_TestFailedEvent(MessageHandlerArgs<ITestFailed> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"TestFailed event, method: {args.Message.Test.TestCase.TestMethod.Method.Name}");
            Console.WriteLine($"TestFailed event, messages: \n {string.Join(",", args.Message.Messages)}");
        }
    }

    private static void ExecutionEvenSink_TestPassedEvent(MessageHandlerArgs<ITestPassed> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"TestPassed event: {args.Message.Test.DisplayName}");
        }
    }

    private static void ExecutionEvenSink_TestAssemblyStartingEvent(MessageHandlerArgs<ITestAssemblyStarting> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine(
                $"Starting test run at: {args.Message.StartTime} with test env: {args.Message.TestFrameworkDisplayName}");
            Console.WriteLine($"Testing assembly: {args.Message.TestAssembly.Assembly.Name}");
            Console.WriteLine($"Test cases count: {args.Message.TestCases.Count()}");
        }
    }

    private static void ExecutionEvenSink_TestAssemblyFinishedEvent(MessageHandlerArgs<ITestAssemblyFinished> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"Finished running tests in assembly run: {args.Message.TestAssembly.Assembly.Name}");
            Console.WriteLine($"Tests failed: {args.Message.TestsFailed}");
            Console.WriteLine($"Tests skipped: {args.Message.TestsFailed}");
            Console.WriteLine($"Tests ran: {args.Message.TestsRun}");
            Console.WriteLine($"Total execution time: {args.Message.ExecutionTime} seconds.");
        }

        Finished.Set();
    }
}