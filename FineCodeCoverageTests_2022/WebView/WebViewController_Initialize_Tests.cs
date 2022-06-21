namespace FineCodeCoverageTests.WebView_Tests
{
    using System.IO;
    using System.Linq;
    using System.Windows;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_Initialize_Tests
    {
        private WebViewController webViewController;
        private Mock<IWebView> mockWebView;
        private AutoMoqer mocker;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mocker.SetInstance(Enumerable.Empty<IWebViewHostObjectRegistration>());
            this.mocker.SetInstance(Enumerable.Empty<IPostJson>());
            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("FCCAppDataPath");
            this.webViewController = this.mocker.Create<WebViewController>();

            this.mockWebView = new Mock<IWebView>();
            this.webViewController.Initialize(this.mockWebView.Object);
        }

        [Test]
        public void Should_Set_The_WebView_To_Stretch()
        {
            this.mockWebView.Verify(webView => webView.SetVerticalAlignment(VerticalAlignment.Stretch));
            this.mockWebView.Verify(webView => webView.SetHorizontalAlignment(HorizontalAlignment.Stretch));
        }

        [Test]
        public void Should_Hide_The_WebView() => this.mockWebView.Verify(
            webView => webView.SetVisibility(Visibility.Hidden)
        );

        [Test]
        public void Should_Ensure_The_User_Data_Directory()
        {
            var expectedUserDataDirectory = Path.Combine("FCCAppDataPath", "webview2");
            Assert.That(this.webViewController.UserDataFolder, Is.EqualTo(expectedUserDataDirectory));
            this.mocker.Verify<IFileUtil>(fileUtil => fileUtil.CreateDirectory(expectedUserDataDirectory));
        }
    }
}
