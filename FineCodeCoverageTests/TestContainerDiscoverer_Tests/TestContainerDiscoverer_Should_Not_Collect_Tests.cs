using FineCodeCoverage.Core.Utilities;
using NUnit.Framework;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Core;
using Moq;
using FineCodeCoverage.Engine.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Impl;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestContainerDiscoverer_Should_Not_Collect_Tests : TestContainerDiscoverer_Tests_Base
    {
        private void SetFCCDisabled()
        {
            SetUpOptions(mockOptions => mockOptions.SetupGet(appOptions => appOptions.Enabled).Returns(false));
        }

        #region TestExecutionStarting
        private void Set_FCC_Disabled_And_Raise_TestExecutionStarting()
        {
            SetFCCDisabled();

            RaiseTestExecutionStarting();
        }

        [Test]
        public void When_TestExecutionStarting_And_FCC_Disabled()
        {
            Set_FCC_Disabled_And_Raise_TestExecutionStarting();

            Assert_MsCodeCoverage_Asked_If_Collecting(false);
        }

        [Test]
        public void And_Should_Log_When_TestExecutionStarting_And_FCC_Disabled()
        {
            Set_FCC_Disabled_And_Raise_TestExecutionStarting();

            mocker.Verify<ILogger>(
                logger => logger.Log("Coverage not collected as FCC disabled.")
            );
        }

        [Test]
        public void And_Should_ToolWindow_Log_Warning__When_TestExecutionStarting_And_FCC_Disabled()
        {
            Set_FCC_Disabled_And_Raise_TestExecutionStarting();

            mocker.GetMock<IEventAggregator>().
                AssertSimpleSingleLog("Coverage not collected as FCC disabled.", MessageContext.Warning);
        }

        [Test]
        public void When_Cancelling_And_TestExecutionStarting()
        {
            testContainerDiscoverer.cancelling = true;
            RaiseTestExecutionStarting();

            Assert_MsCodeCoverage_Asked_If_Collecting(false);
        }

        [Test]
        public void Old_Style_Coverage_When_TestExecutionStarting_Ms_Code_Coverage_Not_Collecting_And_RunInParallel_False()
        {
            RaiseTestExecutionStarting();

            mocker.Verify<IOldStyleCoverage>(
                oldStyleCoverage => oldStyleCoverage.CollectCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>()),
                Times.Never()
            );
        }

        #endregion

        #region Test execution finished
        [Test]
        public void When_Cancelling_And_TestExecutionFinished()
        {
            testContainerDiscoverer.cancelling = true;
            
            RaiseTestExecutionFinished();
        }

        [Test]
        public void When_TestExecutionFinished_And_FCC_Disabled()
        {
            SetFCCDisabled();

            RaiseTestExecutionFinished();
        }

        [Test]
        public void When_TestExecutionFinished_And_Running_In_Parallel()
        {
            SetUpOptions(mockApptions => mockApptions.SetupGet(appOptions => appOptions.RunInParallel).Returns(true));
        }

        [Test]
        public void When_TestExecutionFinished_And_Ms_Code_Coverage_Errored_When_IsCollecting()
        {
            mocker.GetMock<IMsCodeCoverageRunSettingsService>().Setup(
                msCodeCoverage => msCodeCoverage.IsCollectingAsync(It.IsAny<ITestOperation>())
            ).ReturnsAsync(MsCodeCoverageCollectionStatus.Error);
        }

        [Test]
        public void When_TestExecutionFinished_And_Coverage_Conditions_Not_Met()
        {
            var operation = new Mock<IOperation>().Object;

            var testOperation = new Mock<ITestOperation>().Object;
            var mockTestOperationFactory = mocker.GetMock<ITestOperationFactory>();
            mockTestOperationFactory.Setup(testOperationFactory => testOperationFactory.Create(operation)).Returns(testOperation);
            
            var mockRunCoverageConditions = mocker.GetMock<IRunCoverageConditions>();
            mockRunCoverageConditions.Setup(runCoverageConditions => runCoverageConditions.Met(testOperation, appOptions)).Returns(false);

            RaiseTestExecutionFinished(operation);
            mockRunCoverageConditions.VerifyAll();
        }

        #endregion

        [TearDown]
        public void Assert_No_Coverage_Collected()
        {
            Assert_Ms_Code_Coverage_Not_Collecting();
            Assert_Old_Style_Not_Collecting();

        }

    }

}