using FineCodeCoverage.Engine.Model;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Engine
{
    internal class ReportResult
    {
        public IFileLineCoverage FileLineCoverage { get; set; }
        public SummaryResult SummaryResult { get; set; }
        public string HotspotsFile { get; set; }
        public string CoberturaFile { get; set; }
    }
}
