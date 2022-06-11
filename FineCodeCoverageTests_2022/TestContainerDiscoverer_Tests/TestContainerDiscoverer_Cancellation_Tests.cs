using Microsoft.VisualStudio.TestWindow.Extensibility;
namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    internal class TestContainerDiscoverer_Cancellation_Tests : TestContainerDiscoverer_Tests_Base
    {
        [Test]
        public void Should_Set_Cancelling_When_TestExecutionCancelling()
        {
            Assert.That(this.TestContainerDiscoverer.cancelling, Is.False);

            this.RaiseTestExecutionCancelling();

            Assert.That(this.TestContainerDiscoverer.cancelling);
        }

        [Test]
        public void Should_Log_When_TestExecutionCanceling()
        {
            this.RaiseTestExecutionCancelling();

            this.Mocker.Verify<ILogger>(logger => logger.Log("Test execution cancelling - running coverage will be cancelled."));
        }

        [Test]
        public void Should_Contextual_Log_To_The_ToolWindow_When_TestExecutionCanceling()
        {
            this.RaiseTestExecutionCancelling();

            this.Mocker.GetMock<IEventAggregator>().
                AssertSimpleSingleLog(
                    "Test execution cancelling - running coverage will be cancelled.",
                    MessageContext.CoverageCancelled
                );
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Log_When_TestExecutionCancelAndFinished_And_Not_Cancelling(bool cancelling)
        {
            this.TestContainerDiscoverer.cancelling = cancelling;
            this.RaiseTestExecutionCancelAndFinished();

            var times = cancelling ? Times.Never() : Times.Once();
            this.Mocker.Verify<ILogger>(
                logger => logger.Log("There has been an issue running tests. See the Tests output window pane."), times);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Contextual_Log_To_The_ToolWindow_When_TestExecutionCancelAndFinished_And_Not_Cancelling(bool cancelling)
        {
            this.TestContainerDiscoverer.cancelling = cancelling;
            this.RaiseTestExecutionCancelAndFinished();

            this.Mocker.GetMock<IEventAggregator>().AssertHasSimpleLogMessage(!cancelling,
                "There has been an issue running tests. See the Tests output window pane.",
                MessageContext.Error
            );
        }

        [Test]
        public void Should_StopCoverage_When_TestExecutionCanceling()
        {
            var mockCoverageService = new Mock<ICoverageService>();
            this.TestContainerDiscoverer.coverageService = mockCoverageService.Object;

            this.RaiseTestExecutionCancelling();

            mockCoverageService.Verify(coverageService => coverageService.StopCoverage());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_StopCoverage_When_TestExecutionCancelAndFinished(bool cancelling)
        {
            var mockCoverageService = new Mock<ICoverageService>();
            this.TestContainerDiscoverer.coverageService = mockCoverageService.Object;

            this.TestContainerDiscoverer.cancelling = cancelling;

            this.RaiseTestExecutionCancelAndFinished();

            var times = cancelling ? Times.Never() : Times.Once();

            mockCoverageService.Verify(coverageService => coverageService.StopCoverage(), times);
        }

        [Test]
        public void Should_Notify_Collecting_Ms_Code_Coverage_When_TestExecutionCancelling()
        {
            var operation = new Mock<IOperation>().Object;
            var testOperation = new Mock<ITestOperation>().Object;
            _ = this.Mocker.GetMock<ITestOperationFactory>()
                .Setup(testOperationFactory => testOperationFactory.Create(operation))
                .Returns(testOperation);
            this.TestContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            this.RaiseTestExecutionCancelling(operation);

            this.Mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverage => msCodeCoverage.TestExecutionNotFinishedAsync(testOperation)
            );
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Notify_Collecting_Ms_Code_Coverage_When_TestExecutionCancelAndFinished(bool cancelling)
        {
            var operation = new Mock<IOperation>().Object;
            var testOperation = new Mock<ITestOperation>().Object;
            _ = this.Mocker.GetMock<ITestOperationFactory>()
                .Setup(testOperationFactory => testOperationFactory.Create(operation))
                .Returns(testOperation);

            this.TestContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            this.TestContainerDiscoverer.cancelling = cancelling;

            this.RaiseTestExecutionCancelAndFinished(operation);

            var times = cancelling ? Times.Never() : Times.Once();
            this.Mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverage => msCodeCoverage.TestExecutionNotFinishedAsync(testOperation),
                times
            );
        }
    }
}
