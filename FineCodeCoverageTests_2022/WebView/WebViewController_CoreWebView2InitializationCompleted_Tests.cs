namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Collections.Generic;
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
    using FineCodeCoverageTests.Test_helpers;
    using Microsoft.Web.WebView2.Core;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_CoreWebView2InitializationCompleted_Tests
    {
        private WebViewController webViewController;
        private Mock<IWebView> mockWebView;
        private AutoMoqer mocker;

        private class HostObjectRegistration1 : IWebViewHostObjectRegistration
        {
            public string Name => "HostObject1";

            public object HostObject => this;
        }

        private class HostObjectRegistration2 : IWebViewHostObjectRegistration
        {
            public string Name => "HostObject2";

            public object HostObject => this;
        }

        private readonly HostObjectRegistration1 hostObjectRegistration1 = new HostObjectRegistration1();
        private readonly HostObjectRegistration2 hostObjectRegistration2 = new HostObjectRegistration2();

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            IEnumerable<IWebViewHostObjectRegistration> hostObjectRegistrations = new List<IWebViewHostObjectRegistration> {
                this.hostObjectRegistration1,
                this.hostObjectRegistration2
            };
            this.mocker.SetInstance(hostObjectRegistrations);
            this.mocker.SetEmptyEnumerable<IPostJson>();
            _ = this.mocker.GetMock<IFileUtil>().Setup(
                fileUtil => fileUtil.CreateFileSystemWatcher(It.IsAny<string>(), It.IsAny<string>())
            ).Returns(new Mock<IFileSystemWatcher>().Object);
            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath()).Returns("");
            _ = this.mocker.GetMock<IReportPathsProvider>()
                .Setup(reportPathsProvider => reportPathsProvider.Provide())
                .Returns(new Mock<IReportPaths>().Object);
            this.mockWebView = new Mock<IWebView>();
        }

        private void InitializeWebViewController_CoreWebView2InitializationCompleted()
        {
            this.webViewController = this.mocker.Create<WebViewController>();
            this.webViewController.Initialize(this.mockWebView.Object);
            this.webViewController.CoreWebView2InitializationCompleted();
        }

        [Test]
        public void Should_Register_Host_Objects()
        {
            this.InitializeWebViewController_CoreWebView2InitializationCompleted();

            this.mockWebView.Verify(
                webView => webView.AddHostObjectToScript(this.hostObjectRegistration1.Name, this.hostObjectRegistration1)
            );
            this.mockWebView.Verify(
                webView => webView.AddHostObjectToScript(this.hostObjectRegistration2.Name, this.hostObjectRegistration2)
            );
        }

        [Test]
        public void Should_Navigate_To_The_Virtual_Host_Navigation_Path()
        {
            var mockReportPaths = new Mock<IReportPaths>();
            var navigationPath = @"C:\Users\user\Html\index.html";
            _ = mockReportPaths.SetupGet(reportPaths => reportPaths.NavigationPath).Returns(navigationPath);
            _ = this.mocker.GetMock<IReportPathsProvider>()
                .Setup(reportPathsProvider => reportPathsProvider.Provide())
                .Returns(mockReportPaths.Object);

            var expectedHtmlDirectory = Path.GetDirectoryName(navigationPath);
            var expectedHtmlPath = $"https://fcc/{Path.GetFileName(navigationPath)}";

            var navigated = false;
            _ = this.mockWebView.Setup(webView => webView.SetVirtualHostNameToFolderMapping(
                  "fcc",
                  expectedHtmlDirectory,
                  It.IsAny<CoreWebView2HostResourceAccessKind>() // todo 
              )).Callback(() => Assert.That(navigated, Is.False));
            _ = this.mockWebView.Setup(webView => webView.Navigate(expectedHtmlPath))
                .Callback(() => navigated = true);

            this.InitializeWebViewController_CoreWebView2InitializationCompleted();

            this.mockWebView.VerifyAll();



        }

    }
}
