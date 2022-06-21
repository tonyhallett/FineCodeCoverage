namespace FineCodeCoverageTests.WebView_Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.JsSerialization;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_IJsonPoster_Tests
    {
        private bool executedOnMainThread;
        private AutoMoqer mocker;

        private async Task PostJsonAsync(Mock<IWebView> mockWebView)
        {
            this.mocker = new AutoMoqer();
            this.mocker.SetInstance(Enumerable.Empty<IWebViewHostObjectRegistration>());
            this.mocker.SetInstance(Enumerable.Empty<IPostJson>());

            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("");
            _ = this.mocker.GetMock<IPayloadSerializer>().Setup(
                payloadSerializer => payloadSerializer.Serialize("type", "message")
            ).Returns("payload_serialized");

            var webViewController = this.mocker.Create<WebViewController>();
            webViewController.ExecuteOnMainThreadAsync = (action) =>
            {
                this.executedOnMainThread = true;
                action();
                return Task.CompletedTask;
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

            this.mocker.GetMock<ILogger>().VerifyNoOtherCalls();

        }

        [Test]
        public async Task Should_Log_All_Other_Exceptions_Async()
        {
            var exception = new Exception("An exception");
            var mockWebView = new Mock<IWebView>();
            _ = mockWebView.Setup(webView => webView.PostWebMessageAsJson(It.IsAny<string>()))
                .Throws(exception);

            await this.PostJsonAsync(mockWebView);

            this.mocker.Verify<ILogger>(logger => logger.LogWithoutTitle("Exception posting web message of type - type", exception));
        }
    }
}
