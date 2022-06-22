namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
    using Microsoft.Web.WebView2.Core;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_CoreWebView2InitializationCompleted_Tests
    {
        private WebViewController webViewController;
        private Mock<IWebView> mockWebView;

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
            this.Initialize();

            this.webViewController.CoreWebView2InitializationCompleted();
        }

        private void Initialize()
        {
            var mocker = new AutoMoqer();
            IEnumerable<IWebViewHostObjectRegistration> hostObjectRegistrations = new List<IWebViewHostObjectRegistration> {
                this.hostObjectRegistration1,
                this.hostObjectRegistration2
            };
            mocker.SetInstance(hostObjectRegistrations);
            mocker.SetInstance(Enumerable.Empty<IPostJson>());
            mocker.GetMock<IFileUtil>().Setup(
                fileUtil => fileUtil.CreateFileSystemWatcher(It.IsAny<string>(), It.IsAny<string>())
            ).Returns(new Mock<IFileSystemWatcher>().Object);
            _ = mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath()).Returns("");
            this.webViewController = mocker.Create<WebViewController>();

            this.mockWebView = new Mock<IWebView>();
            this.webViewController.Initialize(this.mockWebView.Object);
        }

        [Test]
        public void Should_Register_Host_Objects()
        {
            this.mockWebView.Verify(
                webView => webView.AddHostObjectToScript(this.hostObjectRegistration1.Name, this.hostObjectRegistration1)
            );
            this.mockWebView.Verify(
                webView => webView.AddHostObjectToScript(this.hostObjectRegistration2.Name, this.hostObjectRegistration2)
            );
        }

        // todo
        // these should be in sequence...
        // path to be determined
        [Test]
        public void Should_Navigate_To_The_Js_Report() =>
            this.mockWebView.Verify(webView => webView.Navigate(It.IsAny<string>()));

        [Test]
        public void Should_SetVirtualHostNameToFolderMapping() => this.mockWebView.Verify(
                webView => webView.SetVirtualHostNameToFolderMapping(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CoreWebView2HostResourceAccessKind>() // todo 
                )
            );
    }
}
