using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Options;
using System.Threading;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;

namespace FineCodeCoverage.ReportGeneration
{
    [Export(typeof(IReportGeneratorUtil))]
	internal partial class ReportGeneratorUtil : 
		IReportGeneratorUtil
	{
		private readonly ILogger logger;
		private readonly IAppOptionsProvider appOptionsProvider;
        private readonly IReportGenerator reportGenerator;
        private readonly IReportConfigurationFactory reportConfigurationFactory;
        private readonly IHtmlFilesToFolder htmlFilesToFolder;
        private List<string> errorMessages = new List<string>();
        private readonly string capturingReportBuilderPluginAssemblyLocation = typeof(CapturingReportBuilder).Assembly.Location;
    
		[ImportingConstructor]
		public ReportGeneratorUtil(
			ILogger logger,
			IAppOptionsProvider appOptionsProvider,
			IReportGenerator reportGenerator,
			IReportConfigurationFactory reportConfigurationFactory,
			IHtmlFilesToFolder htmlFilesToFolder
		)
		{
			this.appOptionsProvider = appOptionsProvider;
			this.logger = logger;
            this.reportGenerator = reportGenerator;
            this.reportConfigurationFactory = reportConfigurationFactory;
            this.htmlFilesToFolder = htmlFilesToFolder;
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
			string reportOutputFolder, 
			RiskHotspotsAnalysisThresholds RiskHotspotsAnalysisThresholds
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

			return reportGenerator.GenerateReport(reportConfiguration, new Settings(), RiskHotspotsAnalysisThresholds);
        }

		public ReportGeneratorResult Generate(IEnumerable<string> coverOutputFiles, string reportOutputFolder, CancellationToken cancellationToken)
		{
			var unifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml");

            var riskHotspotsAnalysisThresholds = GetHotspotThresholds(appOptionsProvider.Get());
			var reportSuccess = RunReport(coverOutputFiles, reportOutputFolder, riskHotspotsAnalysisThresholds);
			if (!reportSuccess)
			{
				logger.Log(errorMessages);
				errorMessages.Clear();
				throw new Exception("ReportGenerator error");
			}
			htmlFilesToFolder.Collate(reportOutputFolder);
			var reportGeneratorResult = new ReportGeneratorResult { 
				SummaryResult = CapturingReportBuilder.SummaryResult, UnifiedXmlFile = unifiedXmlFile };

            var hotspotsFile = WriteHotspotsToOutputFolder(CapturingReportBuilder.RiskHotspotAnalysisResult.RiskHotspots, reportOutputFolder);
			reportGeneratorResult.HotspotsFile = hotspotsFile;
            
			return reportGeneratorResult;
		}

        private string WriteHotspotsToOutputFolder(IReadOnlyCollection<RiskHotspot> hotspots, string reportOutputFolder)
		{
			/*
				ReportGenerator iterates assembly, class, file, methodmetric
				MethodMetric Metrics of type CodeQuality => MetricStatus. If any exceeded
				a RiskHotspot is created
			*/
			var rootElement = new XElement("Hotspots", hotspots.Select(hotspot =>
			{
				return new XElement("Hotspot",
					new XElement("Assembly", hotspot.Assembly),
					new XElement("Class", hotspot.Class),
					new XElement("Method", hotspot.MethodMetric.FullName),
					new XElement("Line", hotspot.MethodMetric.Line),
					new XElement("Metrics", 
						hotspot.StatusMetrics.Where(statusMetric => statusMetric.Exceeded).Select(statusMetric =>
						{
							return new XElement("Metric",
								new XElement("Name", statusMetric.Metric.Name),
								new XElement("Value", statusMetric.Metric.Value)
							);
						})
					)
				);
			}));

            var hotspotsPath = Path.Combine(reportOutputFolder, "hotspots.xml");

            rootElement.Save(hotspotsPath);
			return hotspotsPath;
			
		}

        private RiskHotspotsAnalysisThresholds GetHotspotThresholds(IAppOptions appOptions) => new RiskHotspotsAnalysisThresholds
        {
            MetricThresholdForCyclomaticComplexity = appOptions.ThresholdForCyclomaticComplexity,
            MetricThresholdForCrapScore = appOptions.ThresholdForCrapScore,
            MetricThresholdForNPathComplexity = appOptions.ThresholdForNPathComplexity
        };
	}
}
