using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Logging;
using FineCodeCoverage.Options;
using System.Threading;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core;
using FineCodeCoverage.Output.JsMessages;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace FineCodeCoverage.Core.ReportGenerator
{
	[Export(typeof(IReportGeneratorUtil))]
	internal partial class ReportGeneratorUtil : IReportGeneratorUtil
	{
		private readonly IAppOptionsProvider appOptionsProvider;
		private readonly IEventAggregator eventAggregator;
		private readonly IReportGenerator reportGenerator;
        private readonly ILogger logger;
        private readonly IReportConfigurationFactory reportConfigurationFactory;
		private readonly string capturingReportBuilderPluginAssemblyLocation;
		internal List<string> errorMessages = new List<string>();

		[ImportingConstructor]
		public ReportGeneratorUtil(
			IAppOptionsProvider appOptionsProvider,
			IEventAggregator eventAggregator,
			IReportGenerator reportGenerator,
			ILogger logger,
			IReportConfigurationFactory reportConfigurationFactory
		)
		{
			this.appOptionsProvider = appOptionsProvider;
            this.eventAggregator = eventAggregator;
            this.reportGenerator = reportGenerator;
			this.logger = logger;
            this.reportConfigurationFactory = reportConfigurationFactory;
            this.reportGenerator.SetLogger((_, errorMessage) =>
			{
				errorMessages.Add(errorMessage);
			});
            capturingReportBuilderPluginAssemblyLocation = typeof(CapturingReportBuilder).Assembly.Location;
		}

		private bool RunReport(IEnumerable<string> coverOutputFiles, string reportOutputFolder, RiskHotspotsAnalysisThresholds RiskHotspotsAnalysisThresholds)
        {
			var settings = new Settings
			{

			};

			IEnumerable<string> sourceDirectories = Enumerable.Empty<string>();
			IEnumerable<string> assemblyFilters = Enumerable.Empty<string>();
			IEnumerable<string> classFilters = Enumerable.Empty<string>();
			IEnumerable<string> fileFilters = Enumerable.Empty<string>();

			var fccReportConfiguration = new FCCReportConfiguration(
				coverOutputFiles,
				reportOutputFolder,
				sourceDirectories,
				null,//can be null
				new List<string> { CapturingReportBuilder.CapturingReportType, "Cobertura" },
				new List<string> { capturingReportBuilderPluginAssemblyLocation },
				assemblyFilters,
				classFilters,
				fileFilters,
				"Error",
				""
			);
            
			IReportConfiguration reportConfiguration = reportConfigurationFactory.Create(
				fccReportConfiguration
			);
			return reportGenerator.GenerateReport(reportConfiguration, settings, RiskHotspotsAnalysisThresholds);

		}

		private void RaiseNewReport(RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds, string reportFilePath)
        {
			eventAggregator.SendMessage(
				new NewReportMessage
				{
					SummaryResult = CapturingReportBuilder.SummaryResult,
					RiskHotspotAnalysisResult = CapturingReportBuilder.RiskHotspotAnalysisResult,
					RiskHotspotsAnalysisThresholds = riskHotspotsAnalysisThresholds,
					ReportFilePath = reportFilePath
				}
			);
		}

		public string Generate(IEnumerable<string> coverOutputFiles, string reportOutputFolder, CancellationToken cancellationToken)
		{
			var unifiedHtmlFile = Path.Combine(reportOutputFolder, "index.html");
			var unifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml");

			var riskHotspotsAnalysisThresholds = HotspotThresholds();
			bool reportSuccess = RunReport(coverOutputFiles, reportOutputFolder, riskHotspotsAnalysisThresholds);
			
            if (!reportSuccess)
            {
				logger.Log(errorMessages);
				errorMessages = new List<string> { };
				throw new Exception("Report Generator error");
            }

			RaiseNewReport(riskHotspotsAnalysisThresholds,unifiedHtmlFile);

			return unifiedXmlFile;
		}

		private RiskHotspotsAnalysisThresholds HotspotThresholds()
        {
			var options = appOptionsProvider.Provide();
			return new RiskHotspotsAnalysisThresholds
			{
				MetricThresholdForCyclomaticComplexity = options.ThresholdForCyclomaticComplexity,
				MetricThresholdForCrapScore = options.ThresholdForCrapScore,
				MetricThresholdForNPathComplexity = options.ThresholdForNPathComplexity,

			};
		}

	}
}
