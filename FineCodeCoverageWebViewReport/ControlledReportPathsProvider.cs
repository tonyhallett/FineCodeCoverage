using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverageWebViewReport
{
    internal class ControlledReportPathsProvider : IReportPathsProvider
    {
        private readonly ReportPaths reportPaths;

        public ControlledReportPathsProvider(ReportPaths reportPaths)
        {
            this.reportPaths = reportPaths;
        }
        public IReportPaths Provide()
        {
            return reportPaths;
        }
    }
}
