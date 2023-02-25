using Xunit.Abstractions;
using Xunit.Sdk;

namespace DatafusLibrary.TestRunner;

public class TestCaseOrderer : ITestCaseOrderer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public TestCaseOrderer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var orderedTestCases = new List<TTestCase>();

        foreach (var testCase in testCases)
        {
            var testClassName = testCase.TestMethod.TestClass.Class.Name;

            if (testClassName.Equals("TemplateGeneratorTest", StringComparison.Ordinal))
                orderedTestCases.Insert(0, testCase);
            if (!testClassName.Equals("TemplateGeneratorTest", StringComparison.Ordinal))
                orderedTestCases.Add(testCase);
        }

        var message = new DiagnosticMessage($"Ordered {orderedTestCases.Count} test cases," +
                                            $" first to run is: {orderedTestCases.First().TestMethod.Method.Name}");

        _diagnosticMessageSink.OnMessage(message);

        return orderedTestCases.ToList();
    }
}