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

namespace FineCodeCoverageTests.FCCEngine_Tests
{
    public class FCCEngine_UI_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            fccEngine = mocker.Create<FCCEngine>();
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(
                disposeAwareTaskRunner => disposeAwareTaskRunner.CreateLinkedCancellationTokenSource()
            ).Returns(new CancellationTokenSource());
        }



        [Test]
        public void Should_Send_ClearReportMessage_After_Solution_Closes()
        {
            var mockSolutionEvents = mocker.GetMock<ISolutionEvents>();
            mockSolutionEvents.Raise(solutionEvents => solutionEvents.AfterClosing += null, EventArgs.Empty);
            AssertClearsReport();
        }

        [Test]
        public void Should_Send_NewCoverageLinesMessage_With_Null_CoverageLines_When_ClearUI()
        {
            fccEngine.ClearUI();
            AssertClearsCoverageLines();
        }

        [Test]
        public void Should_Send_ClearReportMessage_When_ClearUI()
        {
            fccEngine.ClearUI();
            AssertClearsReport();
        }

        [Test]
        public void Should_Send_NewCoverageLinesMessage_With_Null_CoverageLines_When_FCC_Disabled()
        {
            ChangeEnabledOption(false);
            AssertClearsReport();
        }

        [Test]
        public void Should_Not_Send_NewCoverageLinesMessage_When_FCC_Enabled()
        {
            ChangeEnabledOption(true);
            mocker.Verify<IEventAggregator>(
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
            ChangeEnabledOption(false);
            AssertClearsReport();
        }

        [Test]
        public void Should_Clear_Coverage_Lines_When_RunCancellableCoverageTask()
        {
            fccEngine.RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, null);

            AssertClearsCoverageLines();
        }

        public void AssertClearsReport()
        {
            mocker.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(It.IsAny<ClearReportMessage>(), null));
        }

        private void AssertClearsCoverageLines()
        {
            mocker.Verify<IEventAggregator>(ea =>
                ea.SendMessage(It.Is<NewCoverageLinesMessage>(msg => msg.CoverageLines == null), null)
            );
        }

        private void ChangeEnabledOption(bool enabled)
        {
            var mockAppOptionsProvider = mocker.GetMock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            mockAppOptions.SetupGet(appOptions => appOptions.Enabled).Returns(enabled);
            mockAppOptionsProvider.Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, mockAppOptions.Object);
        }
    }

    internal class FCCEngine_UI_RunCancellableCoverageTask_Success : FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        [Test]
        public async Task Should_Send_NewCoverageLinesMessage()
        {
            var coverageLines = new List<CoverageLine>();
            await RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(coverageLines);
            });


            mocker.Verify<IEventAggregator>(
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