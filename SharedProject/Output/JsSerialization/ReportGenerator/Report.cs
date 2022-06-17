using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
    public class Report : IReport
    {
        public Report(RiskHotspotAnalysisResult riskHotspotAnalysisResult, RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds, SummaryResult summaryResult)
        {
            this.riskHotspotsAnalysisThresholds = riskHotspotsAnalysisThresholds;
            this.summaryResult = new SummaryResultJson(summaryResult);
            var assemblies = this.summaryResult.assemblies;
            this.riskHotspotAnalysisResult = new RiskHotspotAnalysisResultJson(riskHotspotAnalysisResult, assemblies);
        }
        public Report(){ }

        public SummaryResultJson summaryResult { get; set; }
        public RiskHotspotAnalysisResultJson riskHotspotAnalysisResult { get; set; }
        public RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds { get; set; }
    }
}
