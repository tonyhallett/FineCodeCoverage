using System;
using System.ComponentModel.Composition;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace FineCodeCoverage.ReportGeneration
{
    [Export(typeof(IReportGenerator))]
    internal class ReportGeneratorWrapper : IReportGenerator
    {
        private Generator generator = new Generator();
        public bool GenerateReport(
            IReportConfiguration reportConfiguration, 
            Settings settings, 
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds
        )
        {
            return generator.GenerateReport(reportConfiguration, settings, riskHotspotsAnalysisThresholds);
        }
        public void SetLogger(Action<VerbosityLevel, string> logger)
        {
            LoggerFactory.Configure(logger);
        }
    }
}
