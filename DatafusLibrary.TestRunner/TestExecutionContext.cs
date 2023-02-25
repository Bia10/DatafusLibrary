using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.TestRunner;

public class TestExecutionContext
{
    public List<ITestOutput>? TestOutputs;
    public List<ITestFailed>? TestsFailed;
    public List<ITestPassed>? TestsPassed;
    public bool IsFinished;

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
        testMessageSink.Execution.TestFinishedEvent += TestFinishedEvent;
    }

    public void UnsubscribeFromEvents(ref TestMessageSink testMessageSink)
    {
        testMessageSink.Execution.TestOutputEvent -= TestOutputEvent;
        testMessageSink.Execution.TestPassedEvent -= TestPassedEvent;
        testMessageSink.Execution.TestFailedEvent -= TestFailedEvent;
        testMessageSink.Execution.TestFinishedEvent -= TestFinishedEvent;
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

    private void TestFinishedEvent(MessageHandlerArgs<ITestFinished> args)
    {
        if (args.Message is null)
            return;

        IsFinished = true;
    }
}