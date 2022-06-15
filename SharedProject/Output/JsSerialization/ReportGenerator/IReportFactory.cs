using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
    internal interface IReportFactory
    {
        IReport Create(
            RiskHotspotAnalysisResult riskHotspotAnalysisResult,
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds,
            SummaryResult summaryResult
        );
    }
}
