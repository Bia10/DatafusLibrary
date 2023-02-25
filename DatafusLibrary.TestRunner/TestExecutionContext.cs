using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.TestRunner;

public class TestExecutionContext
{
    public bool IsFinished;
    public List<ITestOutput>? TestOutputs;
    public List<ITestFailed>? TestsFailed;
    public List<ITestPassed>? TestsPassed;

    public TestExecutionContext()
    {
        TestsFailed = new List<ITestFailed>();
        TestsPassed = new List<ITestPassed>();
        TestOutputs = new List<ITestOutput>();
    }

    public void SubscribeToEvents(ref TestMessageSink testMessageSink)
    {
        testMessageSink.Execution.TestOutputEvent += TestOutputEvent;
        testMessageSink.Execution.TestPassedEvent += TestPassedEvent;
        testMessageSink.Execution.TestFailedEvent += TestFailedEvent;
        testMessageSink.Execution.TestAssemblyFinishedEvent += TestAssemblyFinished;
    }

    public void UnsubscribeFromEvents(ref TestMessageSink testMessageSink)
    {
        testMessageSink.Execution.TestOutputEvent -= TestOutputEvent;
        testMessageSink.Execution.TestPassedEvent -= TestPassedEvent;
        testMessageSink.Execution.TestFailedEvent -= TestFailedEvent;
        testMessageSink.Execution.TestAssemblyFinishedEvent -= TestAssemblyFinished;
    }

    private void TestFailedEvent(MessageHandlerArgs<ITestFailed> args)
    {
        if (args.Message is null)
            return;

        TestsFailed?.Add(args.Message);
    }

    private void TestPassedEvent(MessageHandlerArgs<ITestPassed> args)
    {
        if (args.Message is null)
            return;

        TestsPassed?.Add(args.Message);
    }

    private void TestOutputEvent(MessageHandlerArgs<ITestOutput> args)
    {
        if (args.Message is null)
            return;

        TestOutputs?.Add(args.Message);
    }

    private void TestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
    {
        if (args.Message is null)
            return;

        IsFinished = true;
    }
}