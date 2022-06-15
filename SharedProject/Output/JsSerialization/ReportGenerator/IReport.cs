using Palmmedia.ReportGenerator.Core.CodeAnalysis;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
    public interface IReport
    {
#pragma warning disable IDE1006 // Naming Styles
        RiskHotspotAnalysisResultJson riskHotspotAnalysisResult { get; set; }
        RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds { get; set; }
        SummaryResultJson summaryResult { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
