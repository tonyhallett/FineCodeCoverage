namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using Moq;
    using NUnit.Framework;

    internal class TestContainerDiscoverer_OperationSetFinished_Should_Reset_State : TestContainerDiscoverer_Tests_Base
    {
        [Test]
        public void RunningInParallel()
        {
            this.TestContainerDiscoverer.runningInParallel = true;

            this.RaiseOperationSetFinished();

            Assert.That(this.TestContainerDiscoverer.runningInParallel, Is.False);
        }

        [Test]
        public void Cancelling()
        {
            this.TestContainerDiscoverer.cancelling = false;

            this.RaiseOperationSetFinished();

            Assert.That(this.TestContainerDiscoverer.cancelling, Is.False);
        }

        [Test]
        public void MsCodeCoverageCollectionStatus_()
        {
            this.TestContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            this.RaiseOperationSetFinished();

            Assert.That(
                this.TestContainerDiscoverer.msCodeCoverageCollectionStatus,
                Is.EqualTo(MsCodeCoverageCollectionStatus.NotCollecting)
            );
        }

        [Test]
        public void CoverageService()
        {
            this.TestContainerDiscoverer.coverageService = new Mock<ICoverageService>().Object;

            this.RaiseOperationSetFinished();

            Assert.That(this.TestContainerDiscoverer.coverageService, Is.Null);
        }

    }

}
