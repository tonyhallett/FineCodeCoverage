using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Output.WebView;
using FineCodeCoverageWebViewReport.InvocationsRecordingRegistration;
using FineCodeCoverageWebViewReport.JsonPosterRegistration;
using FineCodeCoverageWebViewReport.WebViewControllerDependencies;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FineCodeCoverageWebViewReport
{
    internal static class WebViewControllerProvider
    {
        public static IWebViewController Provide(string[] arguments)
        {
            var namedArguments = NamedArguments.Get(arguments);

            var webViewRuntime = new WebViewRuntimeControlledInstalling(namedArguments);

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

            if (!webViewRuntime.IsInstalled)
            {
                webViewHostRegistrations.Add(new WebViewRuntimeHostObjectRegistration(webViewRuntime));
            }

            var jsonPosters = new List<JsonPosterBase> {
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
                webViewRuntime
            );

            if (namedArguments.TryGetValue(NamedArguments.EarlyPostsPath, out var earlyPostsPath))
            {
                var earlyPosts = JsonConvert.DeserializeObject<List<ToEarlyPost>>(File.ReadAllText(earlyPostsPath));
                foreach(var earlyPost in earlyPosts)
                {
                    var earlyPoster = jsonPosters.First(jsonPoster => jsonPoster.Type == earlyPost.Type);
                    earlyPoster.PostJson(earlyPost.PostObject);
                }
            }


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
            var namedArguments = NamedArguments.Get(arguments);
            var hasReportPathsDebug = namedArguments.TryGetValue(NamedArguments.ReportPathsDebug, out var reportPathsDebug);
            if (hasReportPathsDebug)
            {
                reportPathsProvider = new ReportPathsProvider() { debug = bool.Parse(reportPathsDebug) };
            }
            else
            {
                var hasReportPathsPath = namedArguments.TryGetValue(NamedArguments.ReportPathsPath, out var reportPathsPath);
                if (hasReportPathsPath)
                {
                    var serializedReportPaths = File.ReadAllText(reportPathsPath);
                    reportPathsProvider = new ControlledReportPathsProvider(ReportPaths.Deserialize(serializedReportPaths));
                }
                else
                {
                    reportPathsProvider = new ReportPathsProvider() { debug = false };
                }
            }

            return reportPathsProvider;
        }
    }
}
