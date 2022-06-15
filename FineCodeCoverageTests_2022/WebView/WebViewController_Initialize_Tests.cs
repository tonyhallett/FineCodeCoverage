namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Linq;
    using System.Windows;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_Initialize_Tests
    {
        private Mock<IWebView> mockWebView;

        [SetUp]
        public void SetUp()
        {
            var webViewController = new WebViewController(
                Enumerable.Empty<IWebViewHostObjectRegistration>(),
                Enumerable.Empty<IPostJson>(),
                null,
                null
            );

            this.mockWebView = new Mock<IWebView>();
            webViewController.Initialize(this.mockWebView.Object);

        }

        [Test]
        public void Should_Set_The_WebView_To_Stretch()
        {
            this.mockWebView.Verify(webView => webView.SetVerticalAlignment(VerticalAlignment.Stretch));
            this.mockWebView.Verify(webView => webView.SetHorizontalAlignment(HorizontalAlignment.Stretch));
        }

        [Test]
        public void Should_Hide_The_WebView() => this.mockWebView.Verify(webView => webView.SetVisibility(Visibility.Hidden));
    }
}
