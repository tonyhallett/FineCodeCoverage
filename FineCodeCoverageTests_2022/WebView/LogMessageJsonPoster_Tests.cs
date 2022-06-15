namespace FineCodeCoverageTests.WebView_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using FineCodeCoverage.Output.JsPosting;
    using Moq;
    using NUnit.Framework;

    internal class LogMessageJsonPoster_Tests
    {
        private AutoMoqer mocker;
        private LogMessageJsonPoster logMessageJsonPoster;
        private Mock<IJsonPoster> mockJsonPoster;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.logMessageJsonPoster = this.mocker.Create<LogMessageJsonPoster>();

            this.mockJsonPoster = new Mock<IJsonPoster>();
        }

        [Test]
        public void Should_Listen_For_LogMessage() =>
            this.mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.AddListener(this.logMessageJsonPoster, null)
            );

        [Test]
        public void Should_Post_Early_LogMessages_When_Ready()
        {
            var logMessage1 = new LogMessage();
            var logMessage2 = new LogMessage();
            this.logMessageJsonPoster.Handle(logMessage1);
            this.logMessageJsonPoster.Handle(logMessage2);

            this.logMessageJsonPoster.Ready(this.mockJsonPoster.Object, null);

            this.VerifyLogsMessage(logMessage1);
            this.VerifyLogsMessage(logMessage2);
        }

        [Test]
        public void Should_PostJson_LogMessage_When_Receives_And_Ready()
        {
            this.logMessageJsonPoster.Ready(this.mockJsonPoster.Object, null);

            var logMessage = new LogMessage();
            this.logMessageJsonPoster.Handle(logMessage);

            this.VerifyLogsMessage(logMessage);
        }

        [Test]
        public void Should_Not_Refresh()
        {
            this.logMessageJsonPoster.Ready(this.mockJsonPoster.Object, null);

            var logMessage = new LogMessage();
            this.logMessageJsonPoster.Handle(logMessage);
            this.mockJsonPoster.Reset();

            this.logMessageJsonPoster.Refresh();

            this.mockJsonPoster.VerifyNoOtherCalls();
        }

        private void VerifyLogsMessage(LogMessage logMessage) =>
            this.mockJsonPoster.Verify(
                jsonPoster => jsonPoster.PostJson("message", logMessage)
            );
    }
}
