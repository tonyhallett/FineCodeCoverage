using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Output.WebView;
using FineCodeCoverageWebViewReport.InvocationsRecordingRegistration;
using FineCodeCoverageWebViewReport.JsonPosterRegistration;
using FineCodeCoverageWebViewReport.WebViewControllerDependencies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineCodeCoverageWebViewReport
{
    internal static class WebViewControllerProvider
    {
        public static IWebViewController Provide()
        {
            var stylingJsonPosterRegistration = new StylingJsonPosterRegistration();
            var reportJsonPosterRegistration = new ReportJsonPosterRegistration();

            var webViewHostRegistrations = new List<IWebViewHostObjectRegistration>
            {
                stylingJsonPosterRegistration,
                reportJsonPosterRegistration,
                new FCCResourcesNavigatorInvocationsRecordingRegistration(),
                new SourceFileOpenerInvocationsRecordingRegistration()
            };

            var jsonPosters = new List<IPostJson> {
                stylingJsonPosterRegistration,
                reportJsonPosterRegistration
            };

            var webViewController = new WebViewController(
                webViewHostRegistrations,
                jsonPosters,
                new PayloadSerializer(),
                new FileLogger()
            );

            webViewController.ExecuteOnMainThreadAsync = (action) =>
            {
                action();
                return Task.CompletedTask;
            };

            return webViewController;
        }
    }
}
