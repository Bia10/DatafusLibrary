using System.Collections.Concurrent;
using NLog;
using NLog.Targets;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests.Helpers.NLog;

[Target("TestOutput")]
public class TestOutputTarget : TargetWithLayoutHeaderAndFooter
{
    private readonly ConcurrentDictionary<string, ITestOutputHelper> _map = new();

    public void Add(ITestOutputHelper testOutputHelper, string loggerName)
    {
        if (string.IsNullOrWhiteSpace(loggerName))
            throw new ArgumentNullException(nameof(loggerName));

        if (_map.TryAdd(loggerName, testOutputHelper))
            return;

        throw new ArgumentException("LoggerName already in use", nameof(loggerName));
    }

    public bool Remove(string loggerName)
    {
        if (string.IsNullOrWhiteSpace(loggerName))
            throw new ArgumentNullException(nameof(loggerName));

        return _map.TryRemove(loggerName, out var _);
    }

    protected override void Write(LogEventInfo logEvent)
    {
        if (string.IsNullOrEmpty(logEvent.LoggerName))
            return;

        if (!_map.TryGetValue(logEvent.LoggerName, out var testOutputHelper))
            return;

        try
        {
            var message = Layout.Render(logEvent);
            testOutputHelper.WriteLine(message);
        }
        catch (Exception ex)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("TestOutputTarget.Write - Exception: {0}", ex.Message);
        }
    }
}