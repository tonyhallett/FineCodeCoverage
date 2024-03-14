using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
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
        private List<string> errorMessages = new List<string>();
        private readonly string capturingReportBuilderPluginAssemblyLocation = typeof(CapturingReportBuilder).Assembly.Location;
    
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
					errorMessages.Add(message);
				}
				else
				{
					logger.Log(message);
				}
			});
        }

		private bool RunReport(
			IEnumerable<string> coverOutputFiles, 
			string reportOutputFolder
		)
		{
            var reportConfiguration = reportConfigurationFactory.Create(
				new FCCReportConfiguration(
					coverOutputFiles,
					reportOutputFolder,
					Enumerable.Empty<string>(),
					null,
					new List<string> {  "Cobertura", CapturingReportBuilder.CapturingReportType, "HtmlInline_AzurePipelines" },
                    new List<string> { capturingReportBuilderPluginAssemblyLocation},
                    Enumerable.Empty<string>(),
					Enumerable.Empty<string>(),
					Enumerable.Empty<string>(),
					VerbosityLevel.Info.ToString(),
					""
				));

			return reportGenerator.GenerateReport(reportConfiguration, new Settings(), hotspotsService.GetRiskHotspotsAnalysisThresholds());
        }

		public ReportGeneratorResult Generate(IEnumerable<string> coverOutputFiles, string reportOutputFolder, CancellationToken cancellationToken)
		{
			var reportSuccess = RunReport(coverOutputFiles, reportOutputFolder);
			if (!reportSuccess)
			{
				ReportGeneratorFailure();
			}
			
			return ReportGeneratorSuccess(reportOutputFolder);
		}

		private ReportGeneratorResult ReportGeneratorSuccess(string reportOutputFolder)
		{
            htmlFilesToFolder.Collate(reportOutputFolder);

            var reportGeneratorResult = new ReportGeneratorResult
            {
                SummaryResult = CapturingReportBuilder.SummaryResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
                HotspotsFile = Path.Combine(reportOutputFolder, "hotspots.xml")
            };
            hotspotsService.WriteHotspotsToXml(CapturingReportBuilder.RiskHotspotAnalysisResult.RiskHotspots, reportGeneratorResult.HotspotsFile);

            return reportGeneratorResult;
        }

		private void ReportGeneratorFailure()
		{
            logger.Log(errorMessages);
            errorMessages.Clear();
            throw new Exception("ReportGenerator error");
        }
	}
}
