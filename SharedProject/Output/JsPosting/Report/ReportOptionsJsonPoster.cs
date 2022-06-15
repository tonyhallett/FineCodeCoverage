using FineCodeCoverage.Output.WebView;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsPosting
{
    [Export(typeof(IPostJson))]
    internal class ReportOptionsJsonPoster : IPostJson
    {
        private readonly IReportOptionsProvider reportOptionsProvider;
        private IJsonPoster jsonPoster;
        ReportOptions reportOptions;

        [ImportingConstructor]
        public ReportOptionsJsonPoster(IReportOptionsProvider reportOptionsProvider)
        {
            this.reportOptionsProvider = reportOptionsProvider;
        }

        public void Ready(IJsonPoster jsonPoster, IWebViewImpl webViewImpl)
        {
            this.jsonPoster = jsonPoster;
            reportOptionsProvider.ReportOptionsChanged += ReportOptionsProvider_ReportOptionsChanged;
            this.reportOptions = reportOptionsProvider.Provide();
            PostReportOptions();
        }

        private void ReportOptionsProvider_ReportOptionsChanged(object sender, ReportOptions newReportOptions)
        {
            reportOptions = newReportOptions;
            PostReportOptions();
        }

        public void Refresh()
        {
            PostReportOptions();
        }

        private void PostReportOptions()
        {
            jsonPoster.PostJson("reportOptions", reportOptions);
        }
    }

}
