namespace FineCodeCoverageTests.WebView_Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.JsSerialization;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_IJsonPoster_Tests
    {
        private Mock<ILogger> mockLogger;
        private bool executedOnMainThread;

        private async Task PostJsonAsync(Mock<IWebView> mockWebView)
        {
            var mockPayloadSerializer = new Mock<IPayloadSerializer>();
            _ = mockPayloadSerializer.Setup(payloadSerializer => payloadSerializer.Serialize("type", "message"))
                .Returns("payload_serialized");

            this.mockLogger = new Mock<ILogger>();
            var webViewController = new WebViewController(
                Enumerable.Empty<IWebViewHostObjectRegistration>(),
                Enumerable.Empty<IPostJson>(),
                mockPayloadSerializer.Object,
                this.mockLogger.Object
            )
            {
                ExecuteOnMainThreadAsync = (action) =>
                {
                    this.executedOnMainThread = true;
                    action();
                    return Task.CompletedTask;
                }
            };

            webViewController.Initialize(mockWebView.Object);
            webViewController.PostJson("type", "message");

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await webViewController.postJsonTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }

        [Test]
        public async Task Should_Payload_Serialized_PostWebMessageAsJson_On_The_Main_Thread_Async()
        {
            var mockWebView = new Mock<IWebView>();
            await this.PostJsonAsync(mockWebView);

            Assert.That(this.executedOnMainThread, Is.True);

            mockWebView.Verify(webView => webView.PostWebMessageAsJson("payload_serialized"));
        }

        [Test]
        public async Task Should_Not_Log_ObjectDisposedException_Async()
        {
            var mockWebView = new Mock<IWebView>();
            _ = mockWebView.Setup(webView => webView.PostWebMessageAsJson(It.IsAny<string>()))
                .Throws(new ObjectDisposedException(""));

            await this.PostJsonAsync(mockWebView);

            this.mockLogger.VerifyNoOtherCalls();

        }

        [Test]
        public async Task Should_Log_All_Other_Exceptions_Async()
        {
            var exception = new Exception("An exception");
            var mockWebView = new Mock<IWebView>();
            _ = mockWebView.Setup(webView => webView.PostWebMessageAsJson(It.IsAny<string>()))
                .Throws(exception);

            await this.PostJsonAsync(mockWebView);

            this.mockLogger.Verify(logger => logger.LogWithoutTitle("Exception posting web message of type - type", exception));
        }
    }
}
