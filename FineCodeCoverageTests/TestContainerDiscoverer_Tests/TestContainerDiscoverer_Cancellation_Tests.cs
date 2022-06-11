using FineCodeCoverage.Core.Utilities;
using Moq;
using NUnit.Framework;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Core;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using FineCodeCoverage.Impl;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestContainerDiscoverer_Cancellation_Tests : TestContainerDiscoverer_Tests_Base
    {
        [Test]
        public void Should_Set_Cancelling_When_TestExecutionCancelling()
        {
            Assert.IsFalse(testContainerDiscoverer.cancelling);

            RaiseTestExecutionCancelling();

            Assert.IsTrue(testContainerDiscoverer.cancelling);
        }

        [Test]
        public void Should_Log_When_TestExecutionCanceling()
        {
            RaiseTestExecutionCancelling();

            mocker.Verify<ILogger>(logger => logger.Log("Test execution cancelling - running coverage will be cancelled."));
        }

        [Test]
        public void Should_Contextual_Log_To_The_ToolWindow_When_TestExecutionCanceling()
        {
            RaiseTestExecutionCancelling();

            mocker.GetMock<IEventAggregator>().
                AssertSimpleSingleLog("Test execution cancelling - running coverage will be cancelled.", MessageContext.CoverageCancelled);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Log_When_TestExecutionCancelAndFinished_And_Not_Cancelling(bool cancelling)
        {
            testContainerDiscoverer.cancelling = cancelling;
            RaiseTestExecutionCancelAndFinished();

            var times = cancelling ? Times.Never() : Times.Once();
            mocker.Verify<ILogger>(logger => logger.Log("There has been an issue running tests. See the Tests output window pane."),times);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Contextual_Log_To_The_ToolWindow_When_TestExecutionCancelAndFinished_And_Not_Cancelling(bool cancelling)
        {
            testContainerDiscoverer.cancelling = cancelling;
            RaiseTestExecutionCancelAndFinished();

            mocker.GetMock<IEventAggregator>().AssertHasSimpleLogMessage(!cancelling,
                "There has been an issue running tests. See the Tests output window pane.",
                MessageContext.Error
            );
        }

        [Test]
        public void Should_StopCoverage_When_TestExecutionCanceling()
        {
            var mockCoverageService = new Mock<ICoverageService>();
            testContainerDiscoverer.coverageService = mockCoverageService.Object;
            
            RaiseTestExecutionCancelling();

            mockCoverageService.Verify(coverageService => coverageService.StopCoverage());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_StopCoverage_When_TestExecutionCancelAndFinished(bool cancelling)
        {
            var mockCoverageService = new Mock<ICoverageService>();
            testContainerDiscoverer.coverageService = mockCoverageService.Object;

            testContainerDiscoverer.cancelling = cancelling;
            
            RaiseTestExecutionCancelAndFinished();

            Times times = cancelling ? Times.Never() : Times.Once();

            mockCoverageService.Verify(coverageService => coverageService.StopCoverage(),times);
        }

        [Test]
        public void Should_Notify_Collecting_Ms_Code_Coverage_When_TestExecutionCancelling()
        {
            var operation = new Mock<IOperation>().Object;
            var testOperation = new Mock<ITestOperation>().Object;
            mocker.GetMock<ITestOperationFactory>()
                .Setup(testOperationFactory => testOperationFactory.Create(operation))
                .Returns(testOperation);
            testContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            RaiseTestExecutionCancelling(operation);

            mocker.Verify<IMsCodeCoverageRunSettingsService>(msCodeCoverage => msCodeCoverage.TestExecutionNotFinishedAsync(testOperation));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Notify_Collecting_Ms_Code_Coverage_When_TestExecutionCancelAndFinished(bool cancelling)
        {
            var operation = new Mock<IOperation>().Object;
            var testOperation = new Mock<ITestOperation>().Object;
            mocker.GetMock<ITestOperationFactory>()
                .Setup(testOperationFactory => testOperationFactory.Create(operation))
                .Returns(testOperation);

            testContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            testContainerDiscoverer.cancelling = cancelling;

            RaiseTestExecutionCancelAndFinished(operation);

            var times = cancelling ? Times.Never() : Times.Once();
            mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverage => msCodeCoverage.TestExecutionNotFinishedAsync(testOperation),
                times
                );
        }
    }
}