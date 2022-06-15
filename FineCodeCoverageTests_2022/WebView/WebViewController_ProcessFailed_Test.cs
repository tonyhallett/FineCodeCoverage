namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Linq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_ProcessFailed_Test
    {
        private class CoreWebView2ProcessFailedEventArgs
        {
            public override string ToString() => "CoreWebView2ProcessFailed";
        }

        [Test]
        public void Should_Log_ProcessFailed_Exception()
        {

            var mockLogger = new Mock<ILogger>();

            var webViewController = new WebViewController(
                Enumerable.Empty<IWebViewHostObjectRegistration>(),
                Enumerable.Empty<IPostJson>(),
                null,
                mockLogger.Object
            );

            var coreWebView2ProcessFailedEventArgs = new CoreWebView2ProcessFailedEventArgs();
            webViewController.ProcessFailed(coreWebView2ProcessFailedEventArgs);

            mockLogger.Verify(logger => logger.Log("WebView2 Process failed :", coreWebView2ProcessFailedEventArgs));
        }
    }
}
