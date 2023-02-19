using DatafusLibrary.SourceGenerators.Tests.Generation;

namespace DatafusLibrary.TestConsole;

public static class Program
{
    //static string workingDirectory = Environment.CurrentDirectory;
    //static DirectoryInfo projectDirectory = Directory.GetParent(workingDirectory);
    //static DirectoryInfo solutionDirectory = projectDirectory?.Parent?.Parent?.Parent;
    //static string path = solutionDirectory + @"\DatafusLibrary.TestConsole\MockData";

    private static readonly object ConsoleLock = new();
    private static readonly ManualResetEvent Finished = new(false);

    private static Task Main()
    {
        var assemblyLocation = typeof(TemplateGeneratorTest).Assembly.Location;

        var testMessageSink = new TestMessageSink();

        var messagesEventSink = new DiagnosticEventSink();
        messagesEventSink.DiagnosticMessageEvent += MessagesEventSink_DiagnosticMessageEvent;
        messagesEventSink.ErrorMessageEvent += MessagesEventSink_ErrorMessageEvent;

        var executionEventSink = new ExecutionEventSink();
        executionEventSink.TestAssemblyFinishedEvent += ExecutionEvenSink_TestAssemblyFinishedEvent;
        executionEventSink.TestPassedEvent += ExecutionEvenSink_TestPassedEvent;
        executionEventSink.TestFailedEvent += ExecutionEvenSink_TestFailedEvent;
        executionEventSink.TestOutputEvent += ExecutionEventSink_TestOutputEvent;

        var xUnit = new XunitFrontController(AppDomainSupport.IfAvailable, assemblyLocation, diagnosticMessageSink: testMessageSink);

        var assemblyOptions = new TestAssemblyConfiguration
        {
            DiagnosticMessages = true,
            InternalDiagnosticMessages = true,
            MethodDisplay = TestMethodDisplay.ClassAndMethod
        };

        var testDiscoverySink = new TestDiscoverySink();
        var discoveryOptions = TestFrameworkOptions.ForDiscovery(assemblyOptions);
        var executionOptions = TestFrameworkOptions.ForExecution(assemblyOptions);

        xUnit.Find(true, testDiscoverySink, discoveryOptions);
        xUnit.RunAll(executionEventSink, discoveryOptions, executionOptions);
        
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

    private static void MessagesEventSink_ErrorMessageEvent(MessageHandlerArgs<IErrorMessage> args)
    {
        lock (ConsoleLock)
            Console.WriteLine($"ErrorMessage event: {args.Message.Messages}");
    }

    private static void MessagesEventSink_DiagnosticMessageEvent(MessageHandlerArgs<IDiagnosticMessage> args)
    {
        lock (ConsoleLock)
            Console.WriteLine($"DiagnosticMessage event: {args.Message.Message}");
    }

    private static void ExecutionEventSink_TestOutputEvent(MessageHandlerArgs<ITestOutput> args)
    {
        lock (ConsoleLock)
            Console.WriteLine($"TestOutput event: {args.Message.Test.DisplayName}");
    }

    private static void ExecutionEvenSink_TestFailedEvent(MessageHandlerArgs<ITestFailed> args)
    {
        lock (ConsoleLock)
            Console.WriteLine($"TestFailed event: {args.Message.Test.DisplayName}");
    }
 
    private static void ExecutionEvenSink_TestPassedEvent(MessageHandlerArgs<ITestPassed> args)
    {
        lock (ConsoleLock)
            Console.WriteLine($"TestPassed event: {args.Message.Test.DisplayName}");
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