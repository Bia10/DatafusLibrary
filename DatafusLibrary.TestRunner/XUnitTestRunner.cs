using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.TestRunner;

public class XUnitTestRunner
{
    private readonly TestAssemblyConfiguration _assemblyConfiguration;
    private readonly XunitFrontController _frontController;
    private readonly TestMessageSink _testMessageSink;
    public readonly TestDiagnosticContext DiagnosticContext;
    public readonly TestDiscoveryContext DiscoveryContext;
    public readonly TestExecutionContext ExecutionContext;

    public XUnitTestRunner(string testAssemblyPath)
    {
        _testMessageSink = new TestMessageSink();
        DiscoveryContext = new TestDiscoveryContext();
        DiagnosticContext = new TestDiagnosticContext();
        ExecutionContext = new TestExecutionContext();

        DiscoveryContext.SubscribeToEvents(ref _testMessageSink);
        DiagnosticContext.SubscribeToEvents(ref _testMessageSink);
        ExecutionContext.SubscribeToEvents(ref _testMessageSink);

        _assemblyConfiguration = new TestAssemblyConfiguration
        {
            AppDomain = AppDomainSupport.Denied,
            DiagnosticMessages = true,
            InternalDiagnosticMessages = true,
            MethodDisplay = TestMethodDisplay.ClassAndMethod
        };

        _frontController = new XunitFrontController(
            AppDomainSupport.Denied,
            testAssemblyPath,
            null,
            false,
            null,
            null,
            _testMessageSink);
    }

    public void DiscoverTests()
    {
        var discoveryOptions = TestFrameworkOptions.ForDiscovery(_assemblyConfiguration);

        _frontController.Find(true, _testMessageSink, discoveryOptions);
    }

    public List<ITestCase> OrderTests(IEnumerable<ITestCase> testsToOrder)
    {
        var testCaseOrderer = new TestCaseOrderer(_testMessageSink);

        return testCaseOrderer.OrderTestCases(testsToOrder).ToList();
    }

    public void RunTests(IEnumerable<ITestCase> testsToRun)
    {
        var executionOptions = TestFrameworkOptions.ForExecution(_assemblyConfiguration);

        _frontController.RunTests(testsToRun, _testMessageSink, executionOptions);
    }
}