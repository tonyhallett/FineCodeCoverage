namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ILogger = FineCodeCoverage.Logging.ILogger;
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;


    internal class TestContainerDiscoverer_Should_Not_Collect_Tests : TestContainerDiscoverer_Tests_Base
    {
        private void SetFCCDisabled() => this.SetUpOptions(
            mockOptions => mockOptions.SetupGet(appOptions => appOptions.Enabled).Returns(false)
        );

        #region TestExecutionStarting
        private void Set_FCC_Disabled_And_Raise_TestExecutionStarting()
        {
            this.SetFCCDisabled();

            this.RaiseTestExecutionStarting();
        }

        [Test]
        public void When_TestExecutionStarting_And_FCC_Disabled()
        {
            this.Set_FCC_Disabled_And_Raise_TestExecutionStarting();

            this.Assert_MsCodeCoverage_Asked_If_Collecting(false);
        }

        [Test]
        public void And_Should_Log_When_TestExecutionStarting_And_FCC_Disabled()
        {
            this.Set_FCC_Disabled_And_Raise_TestExecutionStarting();

            this.Mocker.Verify<ILogger>(
                logger => logger.Log("Coverage not collected as FCC disabled.")
            );
        }

        [Test]
        public void And_Should_ToolWindow_Log_Warning__When_TestExecutionStarting_And_FCC_Disabled()
        {
            this.Set_FCC_Disabled_And_Raise_TestExecutionStarting();

            this.Mocker.GetMock<IEventAggregator>().
                AssertSimpleSingleLog("Coverage not collected as FCC disabled.", MessageContext.Warning);
        }

        [Test]
        public void When_Cancelling_And_TestExecutionStarting()
        {
            this.TestContainerDiscoverer.cancelling = true;
            this.RaiseTestExecutionStarting();

            this.Assert_MsCodeCoverage_Asked_If_Collecting(false);
        }

        [Test]
        public void Old_Style_Coverage_When_TestExecutionStarting_Ms_Code_Coverage_Not_Collecting_And_RunInParallel_False()
        {
            this.RaiseTestExecutionStarting();

            this.Mocker.Verify<IOldStyleCoverage>(
                oldStyleCoverage => oldStyleCoverage.CollectCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>()),
                Times.Never()
            );
        }

        #endregion

        #region Test execution finished
        [Test]
        public void When_Cancelling_And_TestExecutionFinished()
        {
            this.TestContainerDiscoverer.cancelling = true;

            this.RaiseTestExecutionFinished();
        }

        [Test]
        public void When_TestExecutionFinished_And_FCC_Disabled()
        {
            this.SetFCCDisabled();

            this.RaiseTestExecutionFinished();
        }

        [Test]
        public void When_TestExecutionFinished_And_Running_In_Parallel() =>
            this.SetUpOptions(mockApptions => mockApptions.SetupGet(appOptions => appOptions.RunInParallel).Returns(true));

        [Test]
        public void When_TestExecutionFinished_And_Ms_Code_Coverage_Errored_When_IsCollecting() => _ =
            this.Mocker.GetMock<IMsCodeCoverageRunSettingsService>().Setup(
                msCodeCoverage => msCodeCoverage.IsCollectingAsync(It.IsAny<ITestOperation>())
            ).ReturnsAsync(MsCodeCoverageCollectionStatus.Error);

        [Test]
        public void When_TestExecutionFinished_And_Coverage_Conditions_Not_Met()
        {
            var operation = new Mock<IOperation>().Object;

            var testOperation = new Mock<ITestOperation>().Object;
            var mockTestOperationFactory = this.Mocker.GetMock<ITestOperationFactory>();
            _ = mockTestOperationFactory.Setup(testOperationFactory => testOperationFactory.Create(operation)).Returns(testOperation);

            var mockRunCoverageConditions = this.Mocker.GetMock<IRunCoverageConditions>();
            _ = mockRunCoverageConditions.Setup(runCoverageConditions => runCoverageConditions.Met(testOperation, this.AppOptions)).Returns(false);

            this.RaiseTestExecutionFinished(operation);
            mockRunCoverageConditions.VerifyAll();
        }

        #endregion

        [TearDown]
        public void Assert_No_Coverage_Collected()
        {
            this.Assert_Ms_Code_Coverage_Not_Collecting();
            this.Assert_Old_Style_Not_Collecting();

        }

    }

}
