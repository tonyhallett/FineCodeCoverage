namespace FineCodeCoverageTests.WebView_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.JsSerialization;
    using FineCodeCoverage.Output.WebView;
    using FineCodeCoverageTests.Test_helpers;
    using Moq;
    using NUnit.Framework;

    internal class WebViewController_IJsonPoster_Tests
    {
        private bool executedOnMainThread;
        private AutoMoqer mocker;

        public class PostJson : IPostJson
        {
            public PostJson(string type, NotReadyPostBehaviour notReadyPostBehaviour)
            {
                this.Type = type;
                this.NotReadyPostBehaviour = notReadyPostBehaviour;
            }
            public string Type { get; }

            public NotReadyPostBehaviour NotReadyPostBehaviour { get; }

            public void Initialize(IJsonPoster jsonPoster) { }
            public void Ready(IWebViewImpl webViewImpl) { }
            public void Refresh() { }
        }

        private async Task PostJsonAsync(Mock<IWebView> mockWebView)
        {
            this.mocker = new AutoMoqer();
            this.mocker.SetEmptyEnumerable<IWebViewHostObjectRegistration>();

            var postJson = new PostJson("type", NotReadyPostBehaviour.KeepAll);
            this.mocker.SetInstance<IEnumerable<IPostJson>>(new List<IPostJson> { postJson });

            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("");
            _ = this.mocker.GetMock<IPayloadSerializer>().Setup(
                payloadSerializer => payloadSerializer.Serialize("type", "message")
            ).Returns("payload_serialized");
            _ = this.mocker.GetMock<IReportPathsProvider>()
                 .Setup(reportPathsProvider => reportPathsProvider.Provide())
                 .Returns(new Mock<IReportPaths>().Object);

            var webViewController = this.mocker.Create<WebViewController>();
            webViewController.ExecuteOnMainThreadAsync = (action) =>
            {
                this.executedOnMainThread = true;
                action();
                return Task.CompletedTask;
            };

            webViewController.Initialize(mockWebView.Object);
            webViewController.CoreWebView2InitializationCompleted();
            webViewController.PostJson("type", "message");

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await webViewController.postJsonTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }

        [Test]
        public async Task Should_Payload_Serialized_PostWebMessageAsJson_On_The_Main_Thread_When_Navigated_Async()
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

        [Test]
        public async Task Should_Post_Early_Jsons_When_Navigate_According_To_Timing_And_NotReadyPostBehavior_Async()
        {
            var mockWebView = new Mock<IWebView>();

            this.mocker = new AutoMoqer();
            this.mocker.SetEmptyEnumerable<IWebViewHostObjectRegistration>();

            var keepAllPostJson = new PostJson("keepAll", NotReadyPostBehaviour.KeepAll);
            var keepLastPostJson = new PostJson("keepLast", NotReadyPostBehaviour.KeepLast);
            var forgetPostJson = new PostJson("forget", NotReadyPostBehaviour.Forget);
            this.mocker.SetInstance<IEnumerable<IPostJson>>(
                new List<IPostJson> { keepAllPostJson, keepLastPostJson, forgetPostJson });

            _ = this.mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("");
            _ = this.mocker.GetMock<IPayloadSerializer>().Setup(
                payloadSerializer => payloadSerializer.Serialize(It.IsAny<string>(), It.IsAny<string>())
            ).Returns<string,string>((type, msg) => $"{type}-{msg}");
            _ = this.mocker.GetMock<IReportPathsProvider>()
                 .Setup(reportPathsProvider => reportPathsProvider.Provide())
                 .Returns(new Mock<IReportPaths>().Object);

            _ = this.mocker.GetMock<IWebViewRuntime>().SetupGet(webViewRuntime => webViewRuntime.IsInstalled).Returns(true);

            var webViewController = this.mocker.Create<WebViewController>();
            webViewController.ExecuteOnMainThreadAsync = (action) =>
            {
                this.executedOnMainThread = true;
                action();
                return Task.CompletedTask;
            };

            webViewController.Initialize(mockWebView.Object);

            webViewController.PostJson("keepAll", "message1");
            webViewController.PostJson("keepAll", "message2");
            webViewController.PostJson("forget", "forgotten");
            webViewController.PostJson("keepLast", "first");
            webViewController.PostJson("keepLast", "last");
            webViewController.PostJson("keepAll", "message3");

            webViewController.CoreWebView2InitializationCompleted();
            mockWebView.Raise(webView => webView.DomContentLoaded += null, null, null);

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await webViewController.postJsonTask;

            var postWebMessageAsJsonInvocationsArg = mockWebView.Invocations.OfType<IInvocation>()
                .Where(invocation => invocation.Method.Name == nameof(IWebView.PostWebMessageAsJson))
                .Select(invocation => invocation.Arguments[0] as string).ToList();

            Assert.That(postWebMessageAsJsonInvocationsArg, Has.Count.EqualTo(4));
            Assert.Multiple(() =>
            {
                Assert.That(postWebMessageAsJsonInvocationsArg[0], Is.EqualTo("keepAll-message1"));
                Assert.That(postWebMessageAsJsonInvocationsArg[1], Is.EqualTo("keepAll-message2"));
                Assert.That(postWebMessageAsJsonInvocationsArg[2], Is.EqualTo("keepLast-last"));
                Assert.That(postWebMessageAsJsonInvocationsArg[3], Is.EqualTo("keepAll-message3"));
            });

        }
    }
}
