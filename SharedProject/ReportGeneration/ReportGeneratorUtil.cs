using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;

namespace FineCodeCoverage.ReportGeneration
{
    [Export(typeof(IReportGeneratorUtil))]
    internal partial class ReportGeneratorUtil : IReportGeneratorUtil
    {
        private readonly ILogger logger;
        private readonly IReportGenerator reportGenerator;
        private readonly IReportConfigurationFactory reportConfigurationFactory;
        private readonly IHtmlFilesToFolder htmlFilesToFolder;
        private readonly IHotspotsService hotspotsService;
        private readonly List<string> errorMessages = new List<string>();
        private readonly string capturingReportBuilderPluginAssemblyLocation = typeof(CapturingReportBuilder).Assembly.Location;
        private readonly Regex fileDoesNotExistAnymoreRegex = new Regex(@"File '.*' does not exist \(any more\)\.", RegexOptions.Compiled);

        [ImportingConstructor]
        public ReportGeneratorUtil(
            ILogger logger,
            IReportGenerator reportGenerator,
            IReportConfigurationFactory reportConfigurationFactory,
            IHtmlFilesToFolder htmlFilesToFolder,
            IHotspotsService hotspotsService
        )
        {
            this.logger = logger;
            this.reportGenerator = reportGenerator;
            this.reportConfigurationFactory = reportConfigurationFactory;
            this.htmlFilesToFolder = htmlFilesToFolder;
            this.hotspotsService = hotspotsService;
            reportGenerator.SetLogger((verbosityLevel, message) =>
            {
                if (verbosityLevel == VerbosityLevel.Error)
                {
                    this.errorMessages.Add(message);
                }
                else
                {
                    Match matched = this.fileDoesNotExistAnymoreRegex.Match(message);
                    if(!matched.Success)
                    {
                        this.logger.Log(message);
                    }
                }
            });
        }

        private bool RunReport(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder
        )
        {
            Palmmedia.ReportGenerator.Core.Reporting.IReportConfiguration reportConfiguration = this.reportConfigurationFactory.Create(
                new FCCReportConfiguration(
                    coverOutputFiles,
                    reportOutputFolder,
                    Enumerable.Empty<string>(),
                    null,
                    new List<string> { "Cobertura", CapturingReportBuilder.CapturingReportType, "HtmlInline_AzurePipelines" },
                    new List<string> { this.capturingReportBuilderPluginAssemblyLocation },
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    VerbosityLevel.Info.ToString(),
                    ""
                ));

            return this.reportGenerator.GenerateReport(reportConfiguration, new Settings(), this.hotspotsService.GetRiskHotspotsAnalysisThresholds());
        }

        public ReportGeneratorResult Generate(IEnumerable<string> coverOutputFiles, string reportOutputFolder, CancellationToken cancellationToken)
        {
            bool reportSuccess = this.RunReport(coverOutputFiles, reportOutputFolder);
            if (!reportSuccess)
            {
                this.ReportGeneratorFailure();
            }

            return this.ReportGeneratorSuccess(reportOutputFolder);
        }

        private ReportGeneratorResult ReportGeneratorSuccess(string reportOutputFolder)
        {
            this.htmlFilesToFolder.Collate(reportOutputFolder);

            var reportGeneratorResult = new ReportGeneratorResult
            {
                SummaryResult = CapturingReportBuilder.SummaryResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
                HotspotsFile = Path.Combine(reportOutputFolder, "hotspots.xml")
            };
            this.hotspotsService.WriteHotspotsToXml(CapturingReportBuilder.RiskHotspotAnalysisResult.RiskHotspots, reportGeneratorResult.HotspotsFile);

            return reportGeneratorResult;
        }

        private void ReportGeneratorFailure()
        {
            this.logger.Log(this.errorMessages);
            this.errorMessages.Clear();
            throw new Exception("ReportGenerator error");
        }
    }
}
