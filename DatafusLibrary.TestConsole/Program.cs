using DatafusLibrary.SourceGenerators.Tests.Generation;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.TestConsole;

public static class Program
{
    private static readonly object ConsoleLock = new();
    private static readonly ManualResetEvent Finished = new(false);

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

        var discoveryOptions = TestFrameworkOptions.ForDiscovery(assemblyOptions);
        var executionOptions = TestFrameworkOptions.ForExecution(assemblyOptions);

        xUnit.Find(true, testMessageSink, discoveryOptions);
        xUnit.RunAll(testMessageSink, discoveryOptions, executionOptions);

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

    private static void DiscoveryEventSink_TestCaseDiscoveryMessageEvent(
        MessageHandlerArgs<ITestCaseDiscoveryMessage> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine(
                $"TestDiscovery event, \n found test: {args.Message.TestCase.TestMethod.Method.Name} \n in class: {args.Message.TestClass.Class.Name}");
        }
    }

    private static void DiscoveryEventSink_DiscoveryCompleteMessageEvent(
        MessageHandlerArgs<IDiscoveryCompleteMessage> args)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"DiscoveryComplete event at: {DateTime.Now}");
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
            Console.WriteLine($"TestOutput event: {args.Message.Output}");
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
            Console.WriteLine($"Tests failed: {args.Message.TestsFailed}");
            Console.WriteLine($"Tests run: {args.Message.TestsRun}");
        }

        Finished.Set();
    }
}