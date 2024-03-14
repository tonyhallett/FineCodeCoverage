using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace FineCodeCoverage.ReportGeneration
{
    public class CapturingReportBuilder : IReportBuilder
    {
        public string ReportType => CapturingReportType;

        public const string CapturingReportType = "CapturingReport";

        public IReportContext ReportContext { get; set; }

        public void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses) { }
    
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            SummaryResult = summaryResult;
            RiskHotspotAnalysisResult = ReportContext.RiskHotspotAnalysisResult;
        }

        public static SummaryResult SummaryResult { get; set; }
        public static RiskHotspotAnalysisResult RiskHotspotAnalysisResult { get; set; }
    }
}
