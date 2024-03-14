using AutoMoq;
using Moq;
using NUnit.Framework;
using FineCodeCoverage.ReportGeneration;
using System;
using Palmmedia.ReportGenerator.Core.Logging;
using System.Threading;
using Palmmedia.ReportGenerator.Core.Reporting;
using Palmmedia.ReportGenerator.Core;
using IReportGenerator = FineCodeCoverage.ReportGeneration.IReportGenerator;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace FineCodeCoverageTests.ReportGeneration
{
    internal class ReportGeneratorUtil_Tests
    {
        private AutoMoqer autoMoqer;
        private readonly string[] coverageOutputFiles = new string[] { "coverageOutputFile" };
        private readonly string reportOutputFolder = "reportOutputFolder";
        private RiskHotspotAnalysisResult riskHotspotAnalysisResult = new RiskHotspotAnalysisResult(new List<RiskHotspot>().AsReadOnly(), false);

        [SetUp]
        public void Setup()
        {
            autoMoqer = new AutoMoqer();
        }

        [Test]
        public void Should_Log_ReportGenerator_Non_Errors()
        {
            var mockReportGenerator = autoMoqer.GetMock<IReportGenerator>();
            Action<VerbosityLevel, string> logCallback = null;
            mockReportGenerator.Setup(reportGenerator => reportGenerator.SetLogger(It.IsAny<Action<VerbosityLevel, string>>()))
                .Callback<Action<VerbosityLevel, string>>(action => logCallback = action);

            var reportGeneratorUtil = autoMoqer.Create<ReportGeneratorUtil>();

            logCallback(VerbosityLevel.Info, "Info");
            logCallback(VerbosityLevel.Warning, "Warning");
            logCallback(VerbosityLevel.Error, "Error");

            autoMoqer.Verify<ILogger>(logger => logger.Log("Info"), Times.Once());
            autoMoqer.Verify<ILogger>(logger => logger.Log("Warning"), Times.Once());
            autoMoqer.Verify<ILogger>(logger => logger.Log("Error"), Times.Never());
        }

        private ReportGeneratorResult GenerateReport(bool success = true,Action<Action<VerbosityLevel, string>> generateReportCallback = null)
        {
            generateReportCallback = generateReportCallback ?? NoOp;
            void NoOp(Action<VerbosityLevel, string> action) { }
            CapturingReportBuilder.RiskHotspotAnalysisResult = riskHotspotAnalysisResult;
            var mockReportGenerator = autoMoqer.GetMock<IReportGenerator>();
            Action<VerbosityLevel, string> logCallback = null;
            mockReportGenerator.Setup(reportGenerator => reportGenerator.SetLogger(It.IsAny<Action<VerbosityLevel, string>>()))
                .Callback<Action<VerbosityLevel, string>>(action => logCallback = action);
            autoMoqer.Setup<IReportGenerator, bool>(reportGenerator => reportGenerator.GenerateReport(
                It.IsAny<IReportConfiguration>(), It.IsAny<Settings>(), It.IsAny<RiskHotspotsAnalysisThresholds>()))
                .Callback(() => generateReportCallback(logCallback))
            .Returns(success);
            
            var reportGeneratorUtil = autoMoqer.Create<ReportGeneratorUtil>();
            return reportGeneratorUtil.Generate(coverageOutputFiles, reportOutputFolder, CancellationToken.None);
        }

        [Test]
        public void Should_GenerateReport_With_VerbosityLevel_Info()
        {
            GenerateReport();
            autoMoqer.Verify<IReportConfigurationFactory>(
               reportConfigurationFactory => reportConfigurationFactory.Create(It.Is<FCCReportConfiguration>(reportConfiguration =>
                          reportConfiguration.VerbosityLevel == VerbosityLevel.Info.ToString()
                   )), Times.Once());
        }

        [Test]
        public void Should_Generate_Report_With_CoverOutputFiles_To_The_ReportOutputFolder()
        {
            GenerateReport();
            autoMoqer.Verify<IReportConfigurationFactory>(
               reportConfigurationFactory => reportConfigurationFactory.Create(It.Is<FCCReportConfiguration>(reportConfiguration =>
                          reportConfiguration.CoverageOutputFiles == coverageOutputFiles && reportConfiguration.TargetDirectory == reportOutputFolder
                   )), Times.Once());
        }

        [Test]
        public void Should_Capture_Results_Via_CapturingReportBuilder_Plugin()
        {
            GenerateReport();
            autoMoqer.Verify<IReportConfigurationFactory>(
               reportConfigurationFactory => reportConfigurationFactory.Create(It.Is<FCCReportConfiguration>(reportConfiguration =>
                          reportConfiguration.ReportTypes.Contains(CapturingReportBuilder.CapturingReportType) &&
                          reportConfiguration.Plugins.Contains(typeof(CapturingReportBuilder).Assembly.Location)
                   )), Times.Once());
        }

        [Test]
        public void Should_Generate_Cobertura_Report()
        {
            GenerateReport();
            autoMoqer.Verify<IReportConfigurationFactory>(
               reportConfigurationFactory => reportConfigurationFactory.Create(It.Is<FCCReportConfiguration>(reportConfiguration =>
                          reportConfiguration.ReportTypes.Contains("Cobertura")
                   )), Times.Once());
        }

        [Test]
        public void Should_Generate_HtmlInline_AzurePipelines_Report()
        {
            GenerateReport();
            autoMoqer.Verify<IReportConfigurationFactory>(
               reportConfigurationFactory => reportConfigurationFactory.Create(It.Is<FCCReportConfiguration>(reportConfiguration =>
                          reportConfiguration.ReportTypes.Contains("HtmlInline_AzurePipelines")
                   )), Times.Once());
        }

        [Test]
        public void Should_Collate_Html_Files_Upon_Success()
        {
            GenerateReport();
            autoMoqer.Verify<IHtmlFilesToFolder>(htmlFilesToFolder => htmlFilesToFolder.Collate(reportOutputFolder), Times.Once());
        }

        [Test]
        public void Should_Write_Hotspots_Upon_Success()
        {
            GenerateReport();
            var expectedPath = Path.Combine(reportOutputFolder, "hotspots.xml");
            autoMoqer.Verify<IHotspotsService>(hotspotsService => 
                hotspotsService.WriteHotspotsToXml(
                    It.Is<IReadOnlyCollection<RiskHotspot>>(riskhotspots => riskhotspots == riskHotspotAnalysisResult.RiskHotspots),
                    expectedPath
                ), Times.Once());
        }

        [Test]
        public void Should_Return_ReportGeneratorResult_Upon_Success()
        {
            var result = GenerateReport();
            Assert.That(result.UnifiedXmlFile, Is.EqualTo(Path.Combine(reportOutputFolder, "Cobertura.xml")));
            Assert.That(result.HotspotsFile, Is.EqualTo(Path.Combine(reportOutputFolder, "hotspots.xml")));
            Assert.That(result.SummaryResult, Is.EqualTo(CapturingReportBuilder.SummaryResult));
        }

        [Test]
        public void Should_Log_Errors_And_Throw_On_Failure()
        {
            var mockLogger = autoMoqer.GetMock<ILogger>();
            mockLogger.Setup(logger => logger.Log(new List<string> { "Error 1" }));
            Assert.That(() => GenerateReport(false,logger =>
            {
                logger(VerbosityLevel.Error, "Error 1");
            }), Throws.Exception.With.Message.EqualTo("ReportGenerator error"));

            mockLogger.VerifyAll();
        }
    }
}
