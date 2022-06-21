namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Linq;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.WebView;
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
            var mocker = new AutoMoqer();
            mocker.SetInstance(Enumerable.Empty<IWebViewHostObjectRegistration>());
            mocker.SetInstance(Enumerable.Empty<IPostJson>());
            var webViewController = mocker.Create<WebViewController>();

            var coreWebView2ProcessFailedEventArgs = new CoreWebView2ProcessFailedEventArgs();
            webViewController.ProcessFailed(coreWebView2ProcessFailedEventArgs);

            mocker.Verify<ILogger>(logger => logger.Log("WebView2 Process failed :", coreWebView2ProcessFailedEventArgs));
        }
    }
}
