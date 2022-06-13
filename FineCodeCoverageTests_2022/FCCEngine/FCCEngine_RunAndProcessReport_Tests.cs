namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.ReportGenerator;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Cobertura;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    public class FCCEngine_RunAndProcessReport_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.fccEngine = this.mocker.Create<FCCEngine>();
        }

        [Test]
        public void Should_Throw_When_No_Coverage_Files() => _ = Assert.Throws<Exception>(
                () => this.fccEngine.RunAndProcessReport(new string[] { }, CancellationToken.None),
                "No coverage output files available for processing"
            );

        [Test]
        public void Should_Throw_If_Cancellation_Requested_Before_GetReportOutputFolder()
        {
            _ = Assert.Throws<OperationCanceledException>(
                () => this.fccEngine.RunAndProcessReport(
                    new string[] { "a.cobertura.xml" },
                    CancellationTokenHelper.GetCancelledCancellationToken()
                )
            );

            this.mocker.Verify<ICoverageToolOutputManager>(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder(),
                Times.Never()
            );
        }

        [Test]
        public void Should_Throw_If_Cancellation_Requested_Before_Generating_Report()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var mockCoverageToolOutputManager = this.mocker.GetMock<ICoverageToolOutputManager>();

            _ = mockCoverageToolOutputManager.Setup(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder()
            ).Callback(() => cancellationTokenSource.Cancel());

            _ = Assert.Throws<OperationCanceledException>(
                () => this.fccEngine.RunAndProcessReport(new string[] { "a.cobertura.xml" }, cancellationToken)
            );

            mockCoverageToolOutputManager.VerifyAll();

        }

        [Test]
        public void Should_Log_Generating_Reports()
        {
            _ = this.fccEngine.RunAndProcessReport(new string[] { "a.cobertura.xml" }, CancellationToken.None);

            this.mocker.Verify<ILogger>(logger => logger.Log("Generating reports"));
            this.mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("Generating reports", MessageContext.ReportGeneratorStart);
        }

        private List<CoverageLine> GenerateReport(
            string[] coverOutputFiles,
            CancellationToken cancellationToken,
            TimeSpan reportDuration = default
        )
        {
            var mockExecutionTimer = this.mocker.GetMock<IExecutionTimer>();
            _ = mockExecutionTimer.Setup(
                executionTimer => executionTimer.Time(It.IsAny<Action>())
            )
            .Callback<Action>(action => action())
            .Returns(reportDuration);

            var reportOutputFolder = "Report output folder";
            var mockCoverageToolOutputManager = this.mocker.GetMock<ICoverageToolOutputManager>();
            _ = mockCoverageToolOutputManager.Setup(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder()
            ).Returns(reportOutputFolder);

            return this.fccEngine.RunAndProcessReport(coverOutputFiles, cancellationToken);
        }

        [Test]
        public void Should_Generate_Report()
        {
            var coverOutputFiles = new string[] { "a.cobertura.xml" };
            var cancellationToken = CancellationToken.None;

            _ = this.GenerateReport(coverOutputFiles, cancellationToken);

            this.mocker.Verify<IReportGeneratorUtil>(
                reportGeneratorUtil => reportGeneratorUtil.Generate(coverOutputFiles, "Report output folder", cancellationToken)
            );
        }

        [Test]
        public void Should_Log_When_Generated_Report_With_Duration()
        {
            var coverOutputFiles = new string[] { "a.cobertura.xml" };
            var cancellationToken = CancellationToken.None;
            var reportDuration = new TimeSpan(0, 1, 2, 3, 4);

            _ = this.GenerateReport(coverOutputFiles, cancellationToken, reportDuration);

            this.mocker.Verify<IReportGeneratorUtil>(
                reportGeneratorUtil => reportGeneratorUtil.Generate(coverOutputFiles, "Report output folder", cancellationToken)
            );

            var expectedGeneratedReportMessage = $"Generated reports - duration {reportDuration.ToStringHoursMinutesSeconds()}";
            this.mocker.Verify<ILogger>(logger => logger.Log(expectedGeneratedReportMessage));
            this.mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog(
                expectedGeneratedReportMessage,
                MessageContext.ReportGeneratorCompleted
            );
        }

        [Test]
        public void Should_Return_The_Processed_Cobertura_File_From_The_ReportGenerator()
        {
            var mockReportGenerator = this.mocker.GetMock<IReportGeneratorUtil>();
            _ = mockReportGenerator.Setup(reportGeneratorUtil =>
                  reportGeneratorUtil.Generate(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).Returns("UnifiedXmlPath");

            var processedCoverageLines = new List<CoverageLine> { };

            var mockCoberturaUtil = this.mocker.GetMock<ICoberturaUtil>();
            _ = mockCoberturaUtil.Setup(coberturaUtil => coberturaUtil.ProcessCoberturaXml("UnifiedXmlPath"))
                .Returns(processedCoverageLines);

            var coverageLines = this.GenerateReport(new string[] { "a.cobertura.xml" }, CancellationToken.None);
            Assert.That(coverageLines, Is.SameAs(processedCoverageLines));
        }

    }
}
