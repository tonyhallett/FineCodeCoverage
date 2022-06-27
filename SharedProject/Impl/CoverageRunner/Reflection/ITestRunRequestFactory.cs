using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Impl
{
    internal interface ITestRunRequestFactory
    {
        TestRunRequest Create(IOperation operation);
    }
}
