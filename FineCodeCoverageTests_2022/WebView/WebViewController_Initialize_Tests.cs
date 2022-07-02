namespace FineCodeCoverageTests.WebView_Tests
{
    using System.IO;
    using System.Linq;
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
    using Moq.Language;
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
            this.mocker.SetEmptyEnumerable<IWebViewHostObjectRegistration>();
            this.mocker.SetEmptyEnumerable<IPostJson>();
            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("FCCAppDataPath");
            _ = this.mocker.GetMock<IReportPathsProvider>()
                .Setup(reportPathsProvider => reportPathsProvider.Provide())
                .Returns(new Mock<IReportPaths>().Object);
            this.webViewController = this.mocker.Create<WebViewController>();
            this.webViewController.ExecuteOnMainThreadAsync = action =>
            {
                action();
                return Task.CompletedTask;
            };
            this.mockWebView = new Mock<IWebView>();
        }

        private void SetWebViewRuntimeInstalled() =>
            _ = this.mocker.GetMock<IWebViewRuntime>().SetupGet(webViewRuntime => webViewRuntime.IsInstalled)
            .Returns(true);

        private void VerifyAddTextBlockWithVisualStudioTextBlockDynamicResourceNames(string text) =>
            this.mockWebView.Verify(
                webView => webView.AddTextBlock(
                    text,
                    Parameter.Is<ITextBlockDynamicResourceNames>().That(
                        Is.InstanceOf<VisualStudioTextBlockDynamicResourceNames>()
                    )
                )
            );

        [Test]
        public void Should_AddTextBlock_Loading_With_VisualStudioTextBlockDynamicResourceNames_When_Already_Installed()
        {
            this.SetWebViewRuntimeInstalled();
            this.webViewController.Initialize(this.mockWebView.Object);

            this.VerifyAddTextBlockWithVisualStudioTextBlockDynamicResourceNames("Loading.");
        }

        [Test]
        public void Should_AddTextBlock_Installing_Web_View_Runtime_With_VisualStudioTextBlockDynamicResourceNames_When_Already_Installed()
        {
            this.webViewController.Initialize(this.mockWebView.Object);

            this.VerifyAddTextBlockWithVisualStudioTextBlockDynamicResourceNames("Installing Web View Runtime.");
        }


        [Test]
        public void Should_Instantiate_When_WebViewRuntime_When_Already_Installed()
        {
            this.SetWebViewRuntimeInstalled();
            this.webViewController.Initialize(this.mockWebView.Object);

            this.mockWebView.Verify(webView => webView.Instantiate());
        }

        [Test]
        public void Should_Instantiate_When_WebView_Notifies_Is_Installed()
        {
            this.webViewController.Initialize(this.mockWebView.Object);

            this.mocker.GetMock<IWebViewRuntime>().Raise(webViewRuntime => webViewRuntime.Installed += null, null, null);

            this.mockWebView.Verify(webView => webView.Instantiate());
        }

        [Test]
        public void Should_UpdateTextBlock_With_Loading_This_Takes_Some_Time_When_WebView_Notifies_Is_Installed()
        {
            this.webViewController.Initialize(this.mockWebView.Object);

            this.mocker.GetMock<IWebViewRuntime>().Raise(webViewRuntime => webViewRuntime.Installed += null, null, null);

            this.mockWebView.Verify(webView => webView.UpdateTextBlock("Loading. This takes some time."));
        }

        private void Should_Set_WebView_Control_Properties_After_Instantiation(params ICallback[] setups)
        {
            this.SetWebViewRuntimeInstalled();
            var webViewInstantiated = false;
            _ = this.mockWebView.Setup(webView => webView.Instantiate()).Callback(() => webViewInstantiated = true);

            setups.ToList().ForEach(setup => setup.Callback(() => Assert.That(webViewInstantiated, Is.True)));

            this.webViewController.Initialize(this.mockWebView.Object);

            this.mockWebView.VerifyAll();
        }

        [Test]
        public void Should_Set_The_WebView_Control_ToStretch_After_Instantiation() =>
            this.Should_Set_WebView_Control_Properties_After_Instantiation(
                this.mockWebView.Setup(webView => webView.SetWebViewVerticalAlignment(VerticalAlignment.Stretch)),
                this.mockWebView.Setup(webView => webView.SetWebViewHorizontalAlignment(HorizontalAlignment.Stretch))
            );

        [Test]
        public void Should_Hide_The_WebView_After_Instantiation() =>
            this.Should_Set_WebView_Control_Properties_After_Instantiation(
                this.mockWebView.Setup(webView => webView.SetWebViewVisibility(Visibility.Hidden))
            );

        [Test]
        public void Should_Ensure_The_User_Data_Directory()
        {
            this.webViewController.Initialize(this.mockWebView.Object);

            var expectedUserDataDirectory = Path.Combine("FCCAppDataPath", "webview2");
            Assert.That(this.webViewController.UserDataFolder, Is.EqualTo(expectedUserDataDirectory));
            this.mocker.Verify<IFileUtil>(fileUtil => fileUtil.CreateDirectory(expectedUserDataDirectory));
        }
    }
}
