namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
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
            _ = this.mockPostJson1.SetupGet(postJson => postJson.Type).Returns("type1");
            this.mockPostJson2 = new Mock<IPostJson>();
            _ = this.mockPostJson2.SetupGet(postJson => postJson.Type).Returns("type2");
            this.mocker.SetInstance<IEnumerable<IPostJson>>(
                new List<IPostJson> { this.mockPostJson1.Object, this.mockPostJson2.Object }
            );
            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("");
            _ = this.mocker.GetMock<IWebViewRuntime>().SetupGet(webViewRuntime => webViewRuntime.IsInstalled)
            .Returns(true);
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
        public void Should_Not_Watch_When_Constructed_When_Should_Not_Watch()
        {
            this.SetupShouldWatch(false);
            this.mockFileUtil.Verify(
                fileUtil => fileUtil.CreateFileSystemWatcher(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never()
            );
        }

        private Mock<IWebView> Reload(bool created = true, bool navigated = true)
        {
            this.SetupShouldWatch();
            var mockWebView = new Mock<IWebView>();
            this.webViewController.Initialize(mockWebView.Object);
            if (navigated)
            {
                this.webViewController.CoreWebView2InitializationCompleted();
            }

            if (created)
            {
                this.mockFileSystemWatcher.Raise(fsw => fsw.Created += null, null, null);
            }
            else
            {
                this.mockFileSystemWatcher.Raise(fsw => fsw.Changed += null, null, null);
            }

            return mockWebView;
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Should_Reload_When_Watcher_Created_Or_Changed_And_Have_Navigated(bool created, bool navigated) =>
            this.Reload(created, navigated).Verify(webView => webView.Reload(), navigated ? Times.Once() : Times.Never());

        [Test]
        public void Should_Refresh_Json_After_Reload()
        {
            var mockWebView = this.Reload();

            mockWebView.Raise(webView => webView.DomContentLoaded += null, null, null);

            this.mockPostJson1.Verify(postJson => postJson.Refresh());
            this.mockPostJson2.Verify(postJson => postJson.Refresh());
        }
    }
}
