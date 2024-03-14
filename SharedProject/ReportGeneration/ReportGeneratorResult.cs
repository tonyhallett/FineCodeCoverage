using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.ReportGeneration
{
    internal class ReportGeneratorResult
    {
        public SummaryResult SummaryResult { get; set; }
        public string UnifiedXmlFile { get; set; }
        public string HotspotsFile { get; set; }
    }
}
