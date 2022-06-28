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

            IReportPathsProvider reportPathsProvider = GetReportPathsProvider(arguments);

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
                reportPathsProvider,
                new WebViewRuntime(
                    new WebViewRuntimeInstallationChecker(new WebViewRuntimeRegistry()), 
                    new WebViewRuntimeInstaller(new ProcessUtil(), new EnvironmentWrapper())
                )
            );

            webViewController.ExecuteOnMainThreadAsync = (action) =>
            {
                Application.Current.Dispatcher.Invoke(action);
                return Task.CompletedTask;
            };

            return webViewController;
        }

        private static IReportPathsProvider GetReportPathsProvider(string[] arguments)
        {
            IReportPathsProvider reportPathsProvider;
            if (arguments.Length > 0)
            {
                var namedArguments = NamedArguments.Get(arguments);
                var hasReportPathsDebug = namedArguments.TryGetValue(NamedArguments.ReportPathsDebug, out var reportPathsDebug);
                if (hasReportPathsDebug)
                {
                    reportPathsProvider = new ReportPathsProvider() { debug = bool.Parse(reportPathsDebug) };
                }
                else
                {
                    var reportPathsPath = namedArguments[NamedArguments.ReportPathsPath];
                    var serializedReportPaths = File.ReadAllText(reportPathsPath);
                    reportPathsProvider = new ControlledReportPathsProvider(ReportPaths.Deserialize(serializedReportPaths));
                }
            }
            else
            {
                reportPathsProvider = new ReportPathsProvider() { debug = false };
            }

            return reportPathsProvider;
        }
    }
}
