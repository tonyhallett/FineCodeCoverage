namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using ILogger = FineCodeCoverage.ILogger;
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    internal class TestContainerDiscoverer_Collect_Tests : TestContainerDiscoverer_Tests_Base
    {
        private void SetupMsCodeCoverageIsCollecting(MsCodeCoverageCollectionStatus msCodeCoverageCollectionStatus) =>
            _ = this.Mocker.GetMock<IMsCodeCoverageRunSettingsService>()
                .Setup(msCodeCoverage => msCodeCoverage.IsCollectingAsync(It.IsAny<ITestOperation>()))
                .ReturnsAsync(msCodeCoverageCollectionStatus);

        private void SetupRunInParallelOption() =>
            this.SetUpOptions(mockAppOptions => mockAppOptions.SetupGet(appOptions => appOptions.RunInParallel).Returns(true));

        private void SetupRunCoverageConditionsMet() => _ = this.Mocker.GetMock<IRunCoverageConditions>().Setup(
                runCoverageConditions => runCoverageConditions.Met(It.IsAny<ITestOperation>(), It.IsAny<IAppOptions>())
            ).Returns(true);

        private Mock<ITestOperation> SetupTestOperationFactory(IOperation operation)
        {
            var mockTestOperation = new Mock<ITestOperation>();
            _ = this.Mocker.GetMock<ITestOperationFactory>()
                .Setup(testOperationFactory => testOperationFactory.Create(operation))
                .Returns(mockTestOperation.Object);

            return mockTestOperation;
        }

        [Test]
        public void Should_Stop_Coverage_When_When_TestExecutionStarting()
        {
            var mockCoverageService = new Mock<ICoverageService>();
            this.TestContainerDiscoverer.coverageService = mockCoverageService.Object;

            this.RaiseTestExecutionStarting();

            mockCoverageService.Verify(coverageService => coverageService.StopCoverage());
        }

        [Test]
        public void Should_Not_Throw_When_Stop_Coverage_When_When_TestExecutionStarting_And_No_Running_Coverage() =>
            this.RaiseTestExecutionStarting();

        [Test]
        public void Should_Ask_If_Ms_Code_Coverage_Is_Collecting_When_TestExecutionStarting()
        {
            var operation = new Mock<IOperation>().Object;
            var testOperation = this.SetupTestOperationFactory(operation).Object;

            this.RaiseTestExecutionStarting(operation);

            this.Mocker.Verify<IMsCodeCoverageRunSettingsService>(msCodeCoverage => msCodeCoverage.IsCollectingAsync(testOperation));
        }

        [TestCase(MsCodeCoverageCollectionStatus.Collecting)]
        [TestCase(MsCodeCoverageCollectionStatus.Error)]
        [TestCase(MsCodeCoverageCollectionStatus.NotCollecting)]
        public void Should_Set_MsCodeCoverageCollectionStatus_When_TestExecutionStarting(MsCodeCoverageCollectionStatus msCodeCoverageCollectionStatus)
        {
            this.SetupMsCodeCoverageIsCollecting(msCodeCoverageCollectionStatus);

            this.RaiseTestExecutionStarting();

            Assert.That(this.TestContainerDiscoverer.msCodeCoverageCollectionStatus, Is.EqualTo(msCodeCoverageCollectionStatus));
        }

        [Test]
        public void Should_Log_With_RunInParallel_Info_If_Not_Collecting_With_Ms_Code_Coverage_And_Not_RunInParallel_When_TestExecutionStarting()
        {
            this.RaiseTestExecutionStarting();

            this.Mocker.Verify<ILogger>(
                logger => logger.Log("Coverage collected when tests finish. RunInParallel option true for immediate"));
        }

        [Test]
        public void Should_Send_Emphasized_RunInParallel_LogMessage_If_Not_Collecting_With_Ms_Code_Coverage_And_Not_RunInParallel_When_TestExecutionStarting()
        {
            this.RaiseTestExecutionStarting();

            this.Mocker.GetMock<IEventAggregator>().AssertLogMessage(logMessage =>
            {
                var match = false;
                if (
                    logMessage.context == MessageContext.Info &&
                    logMessage.message.Length == 3 &&
                    logMessage.message[0] is Emphasized part1 &&
                    logMessage.message[1] is Emphasized part2 &&
                    logMessage.message[2] is Emphasized part3
                )
                {
                    match = part1.message == "Coverage collected when tests finish. " && part1.emphasis == Emphasis.None &&
                        part2.message == "RunInParallel" && part2.emphasis == Emphasis.Italic &&
                        part3.message == " option true for immediate" && part3.emphasis == Emphasis.None;

                }
                return match;
            });
        }


        [Test]
        public void Should_Collect_With_The_MS_Code_Coverage_When_TestExecutionFinished_And_It_Is_Collecting()
        {
            this.TestContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            var operation = new Mock<IOperation>().Object;
            var testOperation = this.SetupTestOperationFactory(operation).Object;
            this.SetupRunCoverageConditionsMet();

            this.RaiseTestExecutionFinished(operation);

            this.Mocker.Verify<IMsCodeCoverageRunSettingsService>(msCodeCoverage => msCodeCoverage.CollectAsync(operation, testOperation));
        }

        [Test]
        public void Should_Set_The_CoverageService_To_Ms_Code_Coverage_When_Collects_With_It()
        {
            this.TestContainerDiscoverer.msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            this.SetupRunCoverageConditionsMet();

            this.RaiseTestExecutionFinished();

            Assert.That(
                this.TestContainerDiscoverer.coverageService,
                Is.SameAs(this.Mocker.GetMock<IMsCodeCoverageRunSettingsService>().Object)
            );
        }

        [Test]
        public void Should_Collect_With_OldStyle_Coverage_When_Ms_Code_Coverage_Not_Collecting_And_RunInParallel()
        {
            this.SetupRunInParallelOption();

            var operation = new Mock<IOperation>().Object;
            var testOperation = this.SetupTestOperationFactory(operation).Object;

            this.RaiseTestExecutionStarting(operation);

            this.Mocker.Verify<IOldStyleCoverage>(
                oldStyleCoverage => oldStyleCoverage.CollectCoverage(testOperation.GetCoverageProjectsAsync)
            );
        }

        [Test]
        public void Should_Set_RunningInParallel_When_Running_In_Parallel()
        {
            this.SetupRunInParallelOption();

            this.RaiseTestExecutionStarting();

            Assert.That(this.TestContainerDiscoverer.runningInParallel, Is.True);
        }

        [Test]
        public void Should_Set_The_CoverageService_To_OldStyle_When_RunInParallel()
        {
            this.SetupRunInParallelOption();

            this.RaiseTestExecutionStarting();

            Assert.That(this.TestContainerDiscoverer.coverageService, Is.SameAs(this.Mocker.GetMock<IOldStyleCoverage>().Object));
        }

        [Test]
        public void Should_Collect_With_OldStyle_Coverage_When_TestExecutionFinished_And_Ms_Code_Coverage_Not_Collecting_And_Not_Running_In_Parallel()
        {
            this.SetupRunCoverageConditionsMet();

            var operation = new Mock<IOperation>().Object;
            var testOperation = this.SetupTestOperationFactory(operation).Object;

            this.RaiseTestExecutionFinished(operation);

            this.Mocker.Verify<IOldStyleCoverage>(
                oldStyleCoverage => oldStyleCoverage.CollectCoverage(testOperation.GetCoverageProjectsAsync)
            );
        }

        [Test]
        public void Should_Set_The_CoverageService_To_OldStyle_When_OldStyle_Collects_Coverage_When_TestExecutionFinished()
        {
            this.SetupRunCoverageConditionsMet();

            this.RaiseTestExecutionFinished();

            Assert.That(
                this.TestContainerDiscoverer.coverageService,
                Is.SameAs(this.Mocker.GetMock<IOldStyleCoverage>().Object)
            );

        }
    }
}
