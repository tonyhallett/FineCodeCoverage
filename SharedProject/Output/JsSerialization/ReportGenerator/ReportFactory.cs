using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
    [Export(typeof(IReportFactory))]
    internal class ReportFactory : IReportFactory
    {
        public IReport Create(
            RiskHotspotAnalysisResult riskHotspotAnalysisResult, 
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds, 
            SummaryResult summaryResult
        )
        {
            return new Report(riskHotspotAnalysisResult, riskHotspotsAnalysisThresholds, summaryResult);
        }
    }
}
