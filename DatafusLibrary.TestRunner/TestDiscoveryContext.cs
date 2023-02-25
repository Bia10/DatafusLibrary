using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.TestRunner;

public class TestDiscoveryContext
{
    public List<ITestCase>? FoundTests;
    public bool IsFinished;

    public TestDiscoveryContext()
    {
        FoundTests = new List<ITestCase>();
    }

    public void SubscribeToEvents(ref TestMessageSink testMessageSink)
    {
        testMessageSink.Discovery.TestCaseDiscoveryMessageEvent += TestCaseDiscoveryMessageEvent;
        testMessageSink.Discovery.DiscoveryCompleteMessageEvent += DiscoveryCompleteMessageEvent;
    }

    public void UnsubscribeFromEvents(ref TestMessageSink testMessageSink)
    {
        testMessageSink.Discovery.TestCaseDiscoveryMessageEvent -= TestCaseDiscoveryMessageEvent;
        testMessageSink.Discovery.DiscoveryCompleteMessageEvent -= DiscoveryCompleteMessageEvent;
    }

    private void DiscoveryCompleteMessageEvent(MessageHandlerArgs<IDiscoveryCompleteMessage> args)
    {
        if (args.Message is null)
            return;

        IsFinished = true;
    }

    private void TestCaseDiscoveryMessageEvent(MessageHandlerArgs<ITestCaseDiscoveryMessage> args)
    {
        if (args.Message is null)
            return;

        FoundTests?.Add(args.Message.TestCase);
    }
}