namespace FineCodeCoverageTests.WebView_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
    using FineCodeCoverageTests.Test_helpers;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_DomContentLoaded_Tests
    {
        private Mock<IWebView> mockWebView;
        private Mock<IPostJson> mockJsonPoster1;
        private Mock<IPostJson> mockJsonPoster2;
        private WebViewController webViewController;

        [SetUp]
        public void SetUp()
        {
            this.mockJsonPoster1 = new Mock<IPostJson>();
            this.mockJsonPoster2 = new Mock<IPostJson>();

            var mocker = new AutoMoqer();
            mocker.SetEmptyEnumerable<IWebViewHostObjectRegistration>();
            IEnumerable<IPostJson> postJsons = new List<IPostJson>
            {
                this.mockJsonPoster1.Object,
                this.mockJsonPoster2.Object
            };
            mocker.SetInstance(postJsons);
            _ = mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath()).Returns("");
            _ = mocker.GetMock<IReportPathsProvider>()
                .Setup(reportPathsProvider => reportPathsProvider.Provide())
                .Returns(new Mock<IReportPaths>().Object);
            _ = mocker.GetMock<IWebViewRuntime>().SetupGet(webViewRuntime => webViewRuntime.IsInstalled)
            .Returns(true);

            this.webViewController = mocker.Create<WebViewController>();
            this.webViewController.ExecuteOnMainThreadAsync = action =>
            {
                action();
                return Task.CompletedTask;
            };
            this.mockWebView = new Mock<IWebView>();
            this.webViewController.Initialize(this.mockWebView.Object);

            this.mockWebView.Raise(webView => webView.DomContentLoaded += null, EventArgs.Empty);

        }

        //[Test]
        //public void Should_Ready_Json_Posters()
        //{
        //    this.mockJsonPoster1.Verify(jsonPoster1 => jsonPoster1.Ready(this.webViewController, this.mockWebView.Object));
        //    this.mockJsonPoster2.Verify(jsonPoster2 => jsonPoster2.Ready(this.webViewController, this.mockWebView.Object));
        //}

        [Test]
        public void Should_Make_The_WebView_Visible() =>
            this.mockWebView.Verify(webView => webView.SetVisibility(Visibility.Visible));
    }
}
