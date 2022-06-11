using System;
using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using FineCodeCoverage.Core.ReportGenerator;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Cobertura;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output.JsMessages.Logging;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.FCCEngine_Tests
{
    public class FCCEngine_RunAndProcessReport_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            fccEngine = mocker.Create<FCCEngine>();
        }

        [Test]
        public void Should_Throw_When_No_Coverage_Files()
        {
            Assert.Throws<Exception>(() => fccEngine.RunAndProcessReport(new string[] { }, CancellationToken.None), "No coverage output files available for processing");
        }

        [Test]
        public void Should_Throw_If_Cancellation_Requested_Before_GetReportOutputFolder()
        {
            Assert.Throws<OperationCanceledException>(
                () => fccEngine.RunAndProcessReport(new string[] { "a.cobertura.xml"}, CancellationTokenHelper.GetCancelledCancellationToken())
            );
            mocker.Verify<ICoverageToolOutputManager>(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder(),
                Times.Never()
            );
        }

        [Test]
        public void Should_Throw_If_Cancellation_Requested_Before_Generating_Report()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var mockCoverageToolOutputManager = mocker.GetMock<ICoverageToolOutputManager>();

            mockCoverageToolOutputManager.Setup(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder()
            ).Callback(() =>
            {
                cancellationTokenSource.Cancel();
            });

            Assert.Throws<OperationCanceledException>(
                () => fccEngine.RunAndProcessReport(new string[] { "a.cobertura.xml" }, cancellationToken)
            );

            mockCoverageToolOutputManager.VerifyAll();

        }

        [Test]
        public void Should_Log_Generating_Reports()
        {
            fccEngine.RunAndProcessReport(new string[] { "a.cobertura.xml" }, CancellationToken.None);
            
            mocker.Verify<ILogger>(logger => logger.Log("Generating reports"));
            mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("Generating reports", MessageContext.ReportGeneratorStart);
        }

        private List<CoverageLine> GenerateReport(
            string[] coverOutputFiles,
            CancellationToken cancellationToken,
            TimeSpan reportDuration = default
        )
        {
            var mockExecutionTimer = mocker.GetMock<IExecutionTimer>();
            mockExecutionTimer.Setup(
                executionTimer => executionTimer.Time(It.IsAny<Action>())
            )
            .Callback<Action>(action => action())
            .Returns(reportDuration);

            var reportOutputFolder = "Report output folder";
            var mockCoverageToolOutputManager = mocker.GetMock<ICoverageToolOutputManager>();
            mockCoverageToolOutputManager.Setup(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder()
            ).Returns(reportOutputFolder);

            return fccEngine.RunAndProcessReport(coverOutputFiles, cancellationToken);
        }

        [Test]
        public void Should_Generate_Report()
        {
            var coverOutputFiles = new string[] { "a.cobertura.xml" };
            var cancellationToken = CancellationToken.None;
            
            GenerateReport(coverOutputFiles, cancellationToken);

            mocker.Verify<IReportGeneratorUtil>(
                reportGeneratorUtil => reportGeneratorUtil.Generate(coverOutputFiles, "Report output folder", cancellationToken)
            );
        }

        [Test]
        public void Should_Log_When_Generated_Report_With_Duration()
        {
            var coverOutputFiles = new string[] { "a.cobertura.xml" };
            var cancellationToken = CancellationToken.None;
            var reportDuration = new TimeSpan(0, 1, 2, 3, 4);

            GenerateReport(coverOutputFiles, cancellationToken,reportDuration);

            mocker.Verify<IReportGeneratorUtil>(
                reportGeneratorUtil => reportGeneratorUtil.Generate(coverOutputFiles, "Report output folder", cancellationToken)
            );

            var expectedGeneratedReportMessage = $"Generated reports - duration {reportDuration.ToStringHoursMinutesSeconds()}";
            mocker.Verify<ILogger>(logger => logger.Log(expectedGeneratedReportMessage));
            mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog(expectedGeneratedReportMessage, MessageContext.ReportGeneratorCompleted);
        }

        [Test]
        public void Should_Return_The_Processed_Cobertura_File_From_The_ReportGenerator()
        {
            var mockReportGenerator = mocker.GetMock<IReportGeneratorUtil>();
            mockReportGenerator.Setup(reportGeneratorUtil =>
                reportGeneratorUtil.Generate(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).Returns("UnifiedXmlPath");

            var processedCoverageLines = new List<CoverageLine> { };

            var mockCoberturaUtil = mocker.GetMock<ICoberturaUtil>();
            mockCoberturaUtil.Setup(coberturaUtil => coberturaUtil.ProcessCoberturaXml("UnifiedXmlPath")).Returns(processedCoverageLines);
            var coverageLines = GenerateReport(new string[] { "a.cobertura.xml"}, CancellationToken.None);
            Assert.AreSame(processedCoverageLines, coverageLines);
        }

    }
}