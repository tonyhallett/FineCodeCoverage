using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Output.WebView;
using FineCodeCoverageWebViewReport.InvocationsRecordingRegistration;
using FineCodeCoverageWebViewReport.JsonPosterRegistration;
using FineCodeCoverageWebViewReport.WebViewControllerDependencies;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FineCodeCoverageWebViewReport
{
    internal static class WebViewControllerProvider
    {
        public static IWebViewController Provide(string[] arguments)
        {
            var stylingJsonPosterRegistration = new StylingJsonPosterRegistration();
            var reportJsonPosterRegistration = new ReportJsonPosterRegistration();
            var logJsonPosterRegistration = new LogJsonPosterRegistration();

            var webViewHostRegistrations = new List<IWebViewHostObjectRegistration>
            {
                stylingJsonPosterRegistration,
                reportJsonPosterRegistration,
                logJsonPosterRegistration,
                new FCCResourcesNavigatorInvocationsRecordingRegistration(),
                new SourceFileOpenerInvocationsRecordingRegistration(),
                new FCCOutputPaneInvocationsRecordingRegistration()
            };

            var jsonPosters = new List<IPostJson> {
                stylingJsonPosterRegistration,
                reportJsonPosterRegistration,
                logJsonPosterRegistration
            };

            var noLogginglogger = new FileLogger();
            var fileUtil = new FileUtil();

            IReportPathsProvider reportPathsProvider = null;
            if (arguments.Length > 0)
            {
                var reportPathsProviderConfigurationArgument = arguments[0].Substring(2);
                var debugParseSuccess = bool.TryParse(reportPathsProviderConfigurationArgument, out var debug);
                if (debugParseSuccess)
                {
                    reportPathsProvider = new ReportPathsProvider() { debug = debug };
                }
                else
                {
                    var serializedReportPaths = File.ReadAllText(reportPathsProviderConfigurationArgument);
                    reportPathsProvider = new ControlledReportPathsProvider(ReportPaths.Deserialize(serializedReportPaths));
                }
            }
            else
            {
                reportPathsProvider = new ReportPathsProvider() { debug = false };
            }

            var webViewController = new WebViewController(
                webViewHostRegistrations,
                jsonPosters,
                new PayloadSerializer(),
                noLogginglogger,
                new AppDataFolder(
                    noLogginglogger, 
                    new NoEnvironmentVariable(),
                    new NoToolsDirectoryAppOptionsProvider(),
                    fileUtil
                ),
                fileUtil,
                reportPathsProvider
            );

            webViewController.ExecuteOnMainThreadAsync = (action) =>
            {
                Application.Current.Dispatcher.Invoke(action);
                return Task.CompletedTask;
            };

            return webViewController;
        }
    }
}
