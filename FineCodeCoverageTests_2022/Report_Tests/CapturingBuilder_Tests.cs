namespace FineCodeCoverageTests.ReportTests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using FineCodeCoverage.Core.ReportGenerator;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    internal class CapturingBuilder_Tests
    {
        private string coberturaPath;
        private string reportOutputFolder;
        private void WriteCobertura()
        {
            var tempPath = Path.GetTempPath();
            this.coberturaPath = Path.Combine(tempPath, "dummy.cobertura.xml");
            File.WriteAllText(this.coberturaPath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE coverage SYSTEM ""http://cobertura.sourceforge.net/xml/coverage-04.dtd"" >
<coverage line-rate=""1"" branch-rate = ""0.5"" lines-covered= ""6"" lines-valid = ""6"" branches-covered = ""1"" branches-valid = ""2"" complexity=""4"" version=""0"" timestamp = ""1653657831"">
  <sources/>
  <packages>
    <package name=""OpenCover"" line-rate = ""1"" branch-rate = ""0.5"" complexity=""3"" >
      <classes>
        <class name=""OpenCover.Class1"" filename=""C:\Users\tonyh\source\repos\OpenCover\OpenCover\Class1.cs"" line-rate=""1"" branch-rate=""0.5"" complexity=""NaN"">
          <methods>
            <method name=""PartialCover"" signature=""(System.Boolean)"" line-rate=""1"" branch-rate=""0.5"" complexity=""NaN"">
              <lines>
                <line number=""12"" hits=""1"" branch=""false"" />
                <line number=""13"" hits=""1"" branch=""true"" condition-coverage=""50% (1/2)"" />
                <line number=""14"" hits=""1"" branch=""false"" />
              </lines>
            </method>
          </methods>
          <lines>
            <line number = ""12"" hits=""1"" branch=""false"" />
            <line number = ""13"" hits=""1"" branch=""true"" condition-coverage=""50% (1/2)"" />
            <line number = ""14"" hits=""1"" branch=""false"" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
");
        }

        private void CreateReportOutputDirectory()
        {
            var tempPath = Path.GetTempPath();
            this.reportOutputFolder = Path.Combine(tempPath, Path.GetRandomFileName());
            _ = Directory.CreateDirectory(this.reportOutputFolder);
        }

        [Test]
        public void Should_Set_Static_Result_Properties_For_NewReportMessage()
        {
            CapturingReportBuilder.SummaryResult = null;
            CapturingReportBuilder.RiskHotspotAnalysisResult = null;

            var mockAppOptionsProvider = new Mock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForCrapScore).Returns(1);
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForCyclomaticComplexity).Returns(2);
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ThresholdForNPathComplexity).Returns(3);
            _ = mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(mockAppOptions.Object);

            var reportGeneratorUtil = new ReportGeneratorUtil(
                mockAppOptionsProvider.Object,
                new Mock<IEventAggregator>().Object,
                new ReportGeneratorWrapper(),
                new Mock<ILogger>().Object,
                new ReportConfigurationFactory()
            );

            this.WriteCobertura();
            this.CreateReportOutputDirectory();

            _ = reportGeneratorUtil.Generate(new List<string> { this.coberturaPath }, this.reportOutputFolder, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(CapturingReportBuilder.SummaryResult, Is.Not.Null);
                Assert.That(CapturingReportBuilder.RiskHotspotAnalysisResult, Is.Not.Null);
            });
        }

        [TearDown]
        public void Delete()
        {
            File.Delete(this.coberturaPath);
            Directory.Delete(this.reportOutputFolder, true);
        }
    }
}
