﻿using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
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
                fileUtil
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
