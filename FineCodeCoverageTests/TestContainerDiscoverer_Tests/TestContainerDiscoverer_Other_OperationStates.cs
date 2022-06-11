using Microsoft.VisualStudio.TestWindow.Extensibility;
using NUnit.Framework;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestContainerDiscoverer_Other_OperationStates : TestContainerDiscoverer_Tests_Base
    {
        [Test]
        public void Should_Not_Throw()
        {
            RaiseOperationStateChanged(TestOperationStates.ChangeDetection);
        }
    }

}