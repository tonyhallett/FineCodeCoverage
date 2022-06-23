using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Impl
{
    internal interface ITestInstantiationPathAware
    {
        void Notify(IOperationState operationState);
    }
}
