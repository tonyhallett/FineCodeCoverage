namespace FineCodeCoverageTests.WebView_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
    using FineCodeCoverageTests.Test_helpers;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_DomContentLoaded_Watcher_Refresh_Tests
    {
        private WebViewController webViewController;
        private IFileSystemWatcher fileSystemWatcher;
        private Mock<IFileUtil> mockFileUtil;
        private Mock<IFileSystemWatcher> mockFileSystemWatcher;
        private Mock<IPostJson> mockPostJson1;
        private Mock<IPostJson> mockPostJson2;
        private AutoMoqer mocker;
        private const string NavigationPath = @"C:\Users\user\Html\index.html";
        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mocker.SetEmptyEnumerable<IWebViewHostObjectRegistration>();
            this.mockPostJson1 = new Mock<IPostJson>();
            this.mockPostJson2 = new Mock<IPostJson>();
            this.mocker.SetInstance<IEnumerable<IPostJson>>(
                new List<IPostJson> { this.mockPostJson1.Object, this.mockPostJson2.Object }
            );
            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("");

            var expectedWatchPath = Path.GetDirectoryName(NavigationPath);
            this.mockFileSystemWatcher = new Mock<IFileSystemWatcher>();
            _ = this.mockFileSystemWatcher.SetupAllProperties();
            this.fileSystemWatcher = this.mockFileSystemWatcher.Object;
            this.mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = this.mockFileUtil
                .Setup(fileUtil => fileUtil.CreateFileSystemWatcher(expectedWatchPath, "watch.txt"))
                .Returns(this.fileSystemWatcher);
        }

        private void SetupShouldWatch(bool shouldWatch = true)
        {
            var mockReportPaths = new Mock<IReportPaths>();
            _ = mockReportPaths.SetupGet(reportPaths => reportPaths.NavigationPath).Returns(NavigationPath);
            _ = mockReportPaths.SetupGet(reportPaths => reportPaths.ShouldWatch).Returns(shouldWatch);
            _ = this.mocker.GetMock<IReportPathsProvider>()
                .Setup(reportPathsProvider => reportPathsProvider.Provide())
                .Returns(mockReportPaths.Object);

            this.webViewController = this.mocker.Create<WebViewController>();
            this.webViewController.ExecuteOnMainThreadAsync = (action) =>
            {
                action();
                return Task.CompletedTask;
            };
        }

        [Test]
        public void Should_Watch_When_Constructed_When_Should_Watch()
        {
            this.SetupShouldWatch();
            Assert.Multiple(() =>
            {
                this.mockFileUtil.VerifyAll();
                Assert.That(this.fileSystemWatcher.EnableRaisingEvents, Is.True);
                Assert.That(this.fileSystemWatcher.IncludeSubdirectories, Is.False);
            });
        }

        [Test]
        public void Should_Not_Watch_When_Constructed_When_Should_Watch()
        {
            this.SetupShouldWatch(false);
            this.mockFileUtil.Verify(
                fileUtil => fileUtil.CreateFileSystemWatcher(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never()
            );
        }

        private Mock<IWebView> Reload(bool shouldReload = true)
        {
            this.SetupShouldWatch();
            var mockWebView = new Mock<IWebView>();
            this.webViewController.Initialize(mockWebView.Object);
            if (shouldReload)
            {
                this.webViewController.CoreWebView2InitializationCompleted();
            }

            this.mockFileSystemWatcher.Raise(fsw => fsw.Created += null, null, null);

            return mockWebView;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Reload_When_Watcher_Created_And_Have_Navigated(bool navigated) =>
            this.Reload(navigated).Verify(webView => webView.Reload(), navigated ? Times.Once() : Times.Never());

        [Test]
        public void Should_Refresh_Json_After_Reload()
        {
            var mockWebView = this.Reload();

            mockWebView.Raise(webView => webView.DomContentLoaded += null, null, null);

            this.mockPostJson1.Verify(postJson => postJson.Refresh());
            this.mockPostJson2.Verify(postJson => postJson.Refresh());
        }
    }

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
            this.webViewController = mocker.Create<WebViewController>();

            this.mockWebView = new Mock<IWebView>();
            this.webViewController.Initialize(this.mockWebView.Object);

            this.mockWebView.Raise(webView => webView.DomContentLoaded += null, EventArgs.Empty);

        }

        [Test]
        public void Should_Ready_Json_Posters()
        {
            this.mockJsonPoster1.Verify(jsonPoster1 => jsonPoster1.Ready(this.webViewController, this.mockWebView.Object));
            this.mockJsonPoster2.Verify(jsonPoster2 => jsonPoster2.Ready(this.webViewController, this.mockWebView.Object));
        }

        [Test]
        public void Should_Make_The_WebView_Visible() =>
            this.mockWebView.Verify(webView => webView.SetVisibility(Visibility.Visible));
    }
}
