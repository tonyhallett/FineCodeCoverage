using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using System.Threading;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core;
using FineCodeCoverage.Output.JsMessages;

namespace FineCodeCoverage.Core.ReportGenerator
{
	[Export(typeof(IReportGeneratorUtil))]
	internal partial class ReportGeneratorUtil : IReportGeneratorUtil
	{
		private readonly IToolFolder toolFolder;
		private readonly IToolZipProvider toolZipProvider;
		private readonly IAppOptionsProvider appOptionsProvider;
        private readonly IEventAggregator eventAggregator;
        private const string zipPrefix = "reportGenerator";
		private const string zipDirectoryName = "reportGenerator";

		public string ReportGeneratorExePath { get; private set; }
		private readonly string capturingReportBuilderPluginAssemblyLocation;

		[ImportingConstructor]
		public ReportGeneratorUtil(
			IToolFolder toolFolder,
			IToolZipProvider toolZipProvider,
			IFileUtil fileUtil,
			IAppOptionsProvider appOptionsProvider,
			IEventAggregator eventAggregator
		)
		{
			this.appOptionsProvider = appOptionsProvider;
			this.toolFolder = toolFolder;
			this.toolZipProvider = toolZipProvider;
            this.eventAggregator = eventAggregator;
			capturingReportBuilderPluginAssemblyLocation = typeof(CapturingReportBuilder).Assembly.Location;
		}
        
		public void Initialize(string appDataFolder, CancellationToken cancellationToken)
		{
			var zipDestination = toolFolder.EnsureUnzipped(appDataFolder, zipDirectoryName, toolZipProvider.ProvideZip(zipPrefix), cancellationToken);
			ReportGeneratorExePath = Directory.GetFiles(zipDestination, "reportGenerator.exe", SearchOption.AllDirectories).FirstOrDefault()
								  ?? Directory.GetFiles(zipDestination, "*reportGenerator*.exe", SearchOption.AllDirectories).FirstOrDefault();
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

			
			ReportConfiguration reportConfiguration = new ReportConfiguration(
				coverOutputFiles,
				reportOutputFolder,
				sourceDirectories,
				null,//can be null
				new List<string> { CapturingReportBuilder.CapturingReportType, "Cobertura" },
				new List<string> { capturingReportBuilderPluginAssemblyLocation },
				assemblyFilters,
				classFilters,
				fileFilters,
				null, // can be null 
				""
			);
			return new Generator().GenerateReport(reportConfiguration, settings, RiskHotspotsAnalysisThresholds);

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

		public Task<string> GenerateAsync(IEnumerable<string> coverOutputFiles, string reportOutputFolder, CancellationToken cancellationToken)
		{
			// todo need to write out based upon the one one with logging and vs colouring
			var unifiedHtmlFile = Path.Combine(reportOutputFolder, "index.html");
			var unifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml");

			var riskHotspotsAnalysisThresholds = HotspotThresholds();
			bool reportSuccess = false;
			try
			{
				reportSuccess = RunReport(coverOutputFiles, reportOutputFolder, riskHotspotsAnalysisThresholds);
			}catch (Exception ex)
            {
				//..... throw ?
				// before would have received logged output from processUtil ?
            }
            if (!reportSuccess)
            {
				throw new Exception("todo");
            }

			RaiseNewReport(riskHotspotsAnalysisThresholds,unifiedHtmlFile);

			return Task.FromResult(unifiedXmlFile);

			
		}

		private RiskHotspotsAnalysisThresholds HotspotThresholds()
        {
			var options = appOptionsProvider.Get();
			return new RiskHotspotsAnalysisThresholds
			{
				MetricThresholdForCyclomaticComplexity = options.ThresholdForCyclomaticComplexity,
				MetricThresholdForCrapScore = options.ThresholdForCrapScore,
				MetricThresholdForNPathComplexity = options.ThresholdForNPathComplexity,

			};
		}

	}
}
