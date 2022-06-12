namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output.JsMessages;
    using Moq;
    using NUnit.Framework;

    public class FCCEngine_UI_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.fccEngine = this.mocker.Create<FCCEngine>();
            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(
                disposeAwareTaskRunner => disposeAwareTaskRunner.CreateLinkedCancellationTokenSource()
            ).Returns(new CancellationTokenSource());
        }



        [Test]
        public void Should_Send_ClearReportMessage_After_Solution_Closes()
        {
            var mockSolutionEvents = this.mocker.GetMock<ISolutionEvents>();
            mockSolutionEvents.Raise(solutionEvents => solutionEvents.AfterClosing += null, EventArgs.Empty);
            this.AssertClearsReport();
        }

        [Test]
        public void Should_Send_NewCoverageLinesMessage_With_Null_CoverageLines_When_ClearUI()
        {
            this.fccEngine.ClearUI();
            this.AssertClearsCoverageLines();
        }

        [Test]
        public void Should_Send_ClearReportMessage_When_ClearUI()
        {
            this.fccEngine.ClearUI();
            this.AssertClearsReport();
        }

        [Test]
        public void Should_Send_NewCoverageLinesMessage_With_Null_CoverageLines_When_FCC_Disabled()
        {
            this.ChangeEnabledOption(false);
            this.AssertClearsReport();
        }

        [Test]
        public void Should_Not_Send_NewCoverageLinesMessage_When_FCC_Enabled()
        {
            this.ChangeEnabledOption(true);
            this.mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.SendMessage(
                    It.IsAny<NewCoverageLinesMessage>(),
                    It.IsAny<Action<Action>>()
                ),
                Times.Never()
            );
        }

        [Test]
        public void Should_Clear_Report_When_FCC_Disabled()
        {
            this.ChangeEnabledOption(false);
            this.AssertClearsReport();
        }

        [Test]
        public void Should_Clear_Coverage_Lines_When_RunCancellableCoverageTask()
        {
            this.fccEngine.RunCancellableCoverageTask((_) => Task.FromResult(new List<CoverageLine>()), null);

            this.AssertClearsCoverageLines();
        }

        private void AssertClearsReport() =>
            this.mocker.Verify<IEventAggregator>(eventAggregator =>
                eventAggregator.SendMessage(It.IsAny<ClearReportMessage>(),
                null)
            );

        private void AssertClearsCoverageLines() =>
            this.mocker.Verify<IEventAggregator>(ea =>
                ea.SendMessage(It.Is<NewCoverageLinesMessage>(msg => msg.CoverageLines == null),
                null)
            );

        private void ChangeEnabledOption(bool enabled)
        {
            var mockAppOptionsProvider = this.mocker.GetMock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(appOptions => appOptions.Enabled).Returns(enabled);
            mockAppOptionsProvider.Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, mockAppOptions.Object);
        }
    }

    internal class FCCEngine_UI_RunCancellableCoverageTask_Success : FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        [Test]
        public async Task Should_Send_NewCoverageLinesMessage_Async()
        {
            var coverageLines = new List<CoverageLine>();
            await this.RunCancellableCoverageTaskAsync((_) => Task.FromResult(coverageLines));


            this.Mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.SendMessage(
                    It.Is<NewCoverageLinesMessage>(
                        newCoverageLinesMessage => newCoverageLinesMessage.CoverageLines == coverageLines
                    ),
                    null
                )
            );
        }
    }
}
