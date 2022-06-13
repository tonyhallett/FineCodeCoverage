
namespace FineCodeCoverageTests.ReportTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using ILogger = FineCodeCoverage.Logging.ILogger;
    using FineCodeCoverage.Core.ReportGenerator;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output.JsMessages;
    using Moq;
    using NUnit.Framework;
    using Palmmedia.ReportGenerator.Core;
    using Palmmedia.ReportGenerator.Core.CodeAnalysis;
    using Palmmedia.ReportGenerator.Core.Logging;
    using Palmmedia.ReportGenerator.Core.Parser.Analysis;
    using Palmmedia.ReportGenerator.Core.Reporting;

    internal class ReportGeneratorUtil_Tests
    {
        public class TestReportConfiguration : IReportConfiguration
        {
            public IReadOnlyCollection<string> ReportFiles => throw new NotImplementedException();

            public string TargetDirectory => throw new NotImplementedException();

            public IReadOnlyCollection<string> SourceDirectories => throw new NotImplementedException();

            public string HistoryDirectory => throw new NotImplementedException();

            public IReadOnlyCollection<string> ReportTypes => throw new NotImplementedException();

            public IReadOnlyCollection<string> Plugins => throw new NotImplementedException();

            public IReadOnlyCollection<string> AssemblyFilters => throw new NotImplementedException();

            public IReadOnlyCollection<string> ClassFilters => throw new NotImplementedException();

            public IReadOnlyCollection<string> FileFilters => throw new NotImplementedException();

            public VerbosityLevel VerbosityLevel => throw new NotImplementedException();

            public string Tag => throw new NotImplementedException();

            public string Title => throw new NotImplementedException();

            public string License => throw new NotImplementedException();

            public IReadOnlyCollection<string> InvalidReportFilePatterns => throw new NotImplementedException();

            public bool VerbosityLevelValid => throw new NotImplementedException();
        }

        private ReportGeneratorUtil reportGeneratorUtil;
        private readonly List<string> coverageOutput = new List<string> { "One.cobertura.xml", "Two.cobertura.xml" };
        private readonly string reportOutputFolder = "ReportOutputFolder";
        private Mock<IAppOptionsProvider> mockAppOptionsProvider;
        private Mock<IEventAggregator> mockEventAggregator;
        private Mock<FineCodeCoverage.Core.ReportGenerator.IReportGenerator> mockReportGenerator;
        private Mock<IReportConfigurationFactory> mockReportConfigurationFactory;
        private Mock<ILogger> mockLogger;
        private readonly IReportConfiguration reportConfiguration = new TestReportConfiguration();

        [SetUp]
        public void Setup()
        {
            CapturingReportBuilder.SummaryResult = null;
            CapturingReportBuilder.RiskHotspotAnalysisResult = null;

            this.SetMocks();
        }
        private void SetMocks()
        {
            this.mockAppOptionsProvider = new Mock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForCrapScore).Returns(1);
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForCyclomaticComplexity).Returns(2);
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForNPathComplexity).Returns(3);
            _ = this.mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(mockAppOptions.Object);

            this.mockEventAggregator = new Mock<IEventAggregator>();
            this.mockReportGenerator = new Mock<FineCodeCoverage.Core.ReportGenerator.IReportGenerator>();
            _ = this.mockReportGenerator.Setup(reportGenerator => reportGenerator.GenerateReport(
                      It.IsAny<IReportConfiguration>(),
                      It.IsAny<Settings>(),
                      It.IsAny<RiskHotspotsAnalysisThresholds>()
                  )).Returns(true);
            this.mockReportConfigurationFactory = new Mock<IReportConfigurationFactory>();
            _ = this.mockReportConfigurationFactory.Setup(reportConfigurationFactory => reportConfigurationFactory.Create(
                  It.IsAny<FCCReportConfiguration>()
                  )
            ).Returns(this.reportConfiguration);
            this.mockLogger = new Mock<ILogger>();
        }

        private NewReportMessage GetNewReportMessage() =>
            this.mockEventAggregator.Invocations[0].Arguments[0] as NewReportMessage;

        private string RunReport()
        {
            this.reportGeneratorUtil = new ReportGeneratorUtil(
                this.mockAppOptionsProvider.Object,
                this.mockEventAggregator.Object,
                this.mockReportGenerator.Object,
                this.mockLogger.Object,
                this.mockReportConfigurationFactory.Object
            );
            return this.reportGeneratorUtil.Generate(this.coverageOutput, this.reportOutputFolder, CancellationToken.None);
        }

        private (FCCReportConfiguration fccReportConfiguration, string unifiedCobertura) RunReportForReportConfigurationAssert()
        {
            var unifiedCobertura = this.RunReport();
            return (this.mockReportConfigurationFactory.Invocations[0].Arguments[0] as FCCReportConfiguration, unifiedCobertura);
        }

        [Test]
        public void Should_Generate_Report_With_ReportConfiguration()
        {
            _ = this.RunReport();
            Assert.That(
                this.mockReportGenerator.Invocations[1].Arguments[0] as IReportConfiguration,
                Is.SameAs(this.reportConfiguration)
            );
        }

        [Test]
        public void Should_Generate_The_Report_Using_The_Coverage_Output()
        {
            var (fccReportConfiguration, _) = this.RunReportForReportConfigurationAssert();
            Assert.That(fccReportConfiguration.ReportFilePatterns, Is.SameAs(this.coverageOutput));
        }

        [Test]
        public void Should_Generate_The_Report_With_Risk_Analysis_Thresholds_From_Options()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForCrapScore).Returns(1);
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForCyclomaticComplexity).Returns(2);
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForNPathComplexity).Returns(3);
            _ = this.mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(mockAppOptions.Object);
            this.mockReportGenerator.Setup(reportGenerator =>
                reportGenerator.GenerateReport(
                    It.IsAny<IReportConfiguration>(),
                    It.IsAny<Settings>(),
                    It.Is<RiskHotspotsAnalysisThresholds>(thresholds =>
                        thresholds.MetricThresholdForCrapScore == 1 &&
                        thresholds.MetricThresholdForCyclomaticComplexity == 2 &&
                        thresholds.MetricThresholdForNPathComplexity == 3
                    )
                )
            ).Returns(true).Verifiable();
            _ = this.RunReport();
            this.mockReportGenerator.Verify();
        }

        [Test]
        public void Should_Generate_The_Report_Without_History()
        {
            var (fccReportConfiguration, _) = this.RunReportForReportConfigurationAssert();
            Assert.That(fccReportConfiguration.HistoryDirectory, Is.Null);
        }

        [Test]
        public void Should_Generate_The_Report_With_Error_Logging()
        {
            var (fccReportConfiguration, _) = this.RunReportForReportConfigurationAssert();
            Assert.That(fccReportConfiguration.VerbosityLevel, Is.EqualTo("Error"));
        }

        [Test]
        public void Should_Generate_The_Report_With_No_Filters()
        {
            _ = this.RunReport();
            var (fccReportConfiguration, _) = this.RunReportForReportConfigurationAssert();

            Assert.Multiple(() =>
            {
                Assert.That(fccReportConfiguration.AssemblyFilters, Is.Empty);
                Assert.That(fccReportConfiguration.ClassFilters, Is.Empty);
                Assert.That(fccReportConfiguration.FileFilters, Is.Empty);
            });
        }

        [Test]
        public void Should_Generate_A_Cobertura_Report()
        {
            var (fccReportConfiguration, unifiedCobertura) = this.RunReportForReportConfigurationAssert();

            Assert.Multiple(() =>
            {
                Assert.That(fccReportConfiguration.TargetDirectory, Is.EqualTo(this.reportOutputFolder));
                Assert.That(fccReportConfiguration.ReportTypes, Has.Member("Cobertura"));
                Assert.That(unifiedCobertura, Is.EqualTo(Path.Combine(this.reportOutputFolder, "Cobertura.xml")));
            });
        }

        [Test]
        public void Should_Use_Plugin_To_Capture_SummaryResult_And_RiskHotspotAnalysisResult()
        {
            var (reportConfiguration, _) = this.RunReportForReportConfigurationAssert();

            Assert.Multiple(() =>
            {
                Assert.That(reportConfiguration.ReportTypes, Has.Member(CapturingReportBuilder.CapturingReportType));
                Assert.That(reportConfiguration.Plugins, Has.Member(typeof(CapturingReportBuilder).Assembly.Location));
            });
        }

        [Test]
        public void Should_Send_NewReportMessage_With_RiskHotspotsAnalysisThresholds()
        {
            _ = this.RunReport();

            var riskHotspotsAnalysisThresholds = this.GetNewReportMessage().RiskHotspotsAnalysisThresholds;
            Assert.Multiple(() =>
            {
                Assert.That(riskHotspotsAnalysisThresholds.MetricThresholdForCrapScore, Is.EqualTo(1));
                Assert.That(riskHotspotsAnalysisThresholds.MetricThresholdForCyclomaticComplexity, Is.EqualTo(2));
                Assert.That(riskHotspotsAnalysisThresholds.MetricThresholdForNPathComplexity, Is.EqualTo(3));
            });
        }

        [Test]
        public void Should_Send_NewReportMessage_With_Report_File_Path()
        {
            _ = this.RunReport();

            Assert.That(
                this.GetNewReportMessage().ReportFilePath,
                Is.EqualTo(Path.Combine(this.reportOutputFolder, "index.html"))
            );
        }

        [Test]
        public void Should_Send_NewReportMessage_With_Captured_SummaryResult_And_RiskHotspotAnalysisResult()
        {
            var summaryResult = new SummaryResult(
                Enumerable.Empty<Assembly>().ToList().AsReadOnly(),
                "",
                false,
                Enumerable.Empty<string>().ToList().AsReadOnly()
            );
            var riskHotspotAnalysisResult = new RiskHotspotAnalysisResult(
                Enumerable.Empty<RiskHotspot>().ToList().AsReadOnly(),
                false
            );
            CapturingReportBuilder.SummaryResult = summaryResult;
            CapturingReportBuilder.RiskHotspotAnalysisResult = riskHotspotAnalysisResult;

            _ = this.RunReport();
            var newReportMessage = this.GetNewReportMessage();

            Assert.Multiple(() =>
            {
                Assert.That(newReportMessage.SummaryResult, Is.SameAs(summaryResult));
                Assert.That(newReportMessage.RiskHotspotAnalysisResult, Is.SameAs(riskHotspotAnalysisResult));
            });
        }

        [Test]
        public void Should_Log_Error_Messages_Clear_And_Throw_If_Not_Successful()
        {
            _ = this.mockReportGenerator.Setup(
                reportGenerator => reportGenerator.GenerateReport(
                    It.IsAny<IReportConfiguration>(),
                    It.IsAny<Settings>(),
                    It.IsAny<RiskHotspotsAnalysisThresholds>()
                )
            ).Returns(false);

            _ = this.mockReportGenerator.Setup(
                reportGenerator => reportGenerator.SetLogger(It.IsAny<Action<VerbosityLevel, string>>())
            ).Callback<Action<VerbosityLevel, string>>(loggerAction =>
            {
                loggerAction(VerbosityLevel.Error, "error1");
                loggerAction(VerbosityLevel.Error, "error2");
            });

            _ = Assert.Throws<Exception>(() => this.RunReport());

            this.mockLogger.Verify(logger => logger.Log(new List<string> { "error1", "error2" }));
            Assert.That(this.reportGeneratorUtil.errorMessages, Is.Empty);
        }

        // looking at settings and parallel ?

    }
}
