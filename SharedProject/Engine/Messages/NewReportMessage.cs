using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Engine.Messages
{
    internal class NewReportMessage
    {
        public NewReportMessage(SummaryResult summaryResult)
        {
            this.SummaryResult = summaryResult;
        }

        public SummaryResult SummaryResult { get; }
    }
}
