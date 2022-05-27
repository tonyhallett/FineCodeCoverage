using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsMessages
{
    internal class NewReportMessage
    {
        public string ReportFilePath { get; set; }

        public SummaryResult SummaryResult { get; set; }
        public RiskHotspotAnalysisResult RiskHotspotAnalysisResult { get; set; }
        public RiskHotspotsAnalysisThresholds RiskHotspotsAnalysisThresholds { get; set; }
    }

}
