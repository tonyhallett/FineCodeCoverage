namespace FineCodeCoverageTests.CoverageRunner_Tests
{
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using Moq;
    using NUnit.Framework;

    internal class CoverageRunner_OperationSetFinished_Should_Reset_State : CoverageRunner_Tests_Base
    {
        [Test]
        public void RunningInParallel()
        {
            this.CoverageRunner.runningInParallel = true;

            this.RaiseOperationSetFinished();

            Assert.That(this.CoverageRunner.runningInParallel, Is.False);
        }

        [Test]
        public void Cancelling()
        {
            this.CoverageRunner.cancelling = false;

            this.RaiseOperationSetFinished();

            Assert.That(this.CoverageRunner.cancelling, Is.False);
        }

        [Test]
        public void MsCodeCoverageCollectionStatus_()
        {
            this.CoverageRunner.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            this.RaiseOperationSetFinished();

            Assert.That(
                this.CoverageRunner.msCodeCoverageCollectionStatus,
                Is.EqualTo(MsCodeCoverageCollectionStatus.NotCollecting)
            );
        }

        [Test]
        public void CoverageService()
        {
            this.CoverageRunner.coverageService = new Mock<ICoverageService>().Object;

            this.RaiseOperationSetFinished();

            Assert.That(this.CoverageRunner.coverageService, Is.Null);
        }

    }

}
