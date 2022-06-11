using FineCodeCoverage.Core;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestContainerDiscoverer_OperationSetFinished_Should_Reset_State : TestContainerDiscoverer_Tests_Base
    { 
        [Test]
        public void RunningInParallel()
        {
            testContainerDiscoverer.runningInParallel = true;

            RaiseOperationSetFinished();

            Assert.IsFalse(testContainerDiscoverer.runningInParallel);
        }

        [Test]
        public void Cancelling()
        {
            testContainerDiscoverer.cancelling = false;

            RaiseOperationSetFinished();

            Assert.IsFalse(testContainerDiscoverer.cancelling);
        }

        [Test]
        public void MsCodeCoverageCollectionStatus_()
        {
            testContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            RaiseOperationSetFinished();

            Assert.AreEqual(MsCodeCoverageCollectionStatus.NotCollecting,testContainerDiscoverer.msCodeCoverageCollectionStatus);
        }

        public void CoverageService()
        {
            testContainerDiscoverer.coverageService = new Mock<ICoverageService>().Object;

            RaiseOperationSetFinished();

            Assert.Null(testContainerDiscoverer.coverageService);
        }

    }

}