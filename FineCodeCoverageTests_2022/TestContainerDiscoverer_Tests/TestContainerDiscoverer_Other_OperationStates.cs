namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using NUnit.Framework;

    internal class TestContainerDiscoverer_Other_OperationStates : TestContainerDiscoverer_Tests_Base
    {
        [Test]
        public void Should_Not_Throw() => this.RaiseOperationStateChanged(TestOperationStates.ChangeDetection);
    }

}
