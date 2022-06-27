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
            this.logMessageJsonPoster.Initialize(this.mockJsonPoster.Object);
        }

        [Test]
        public void Should_Listen_For_LogMessage() =>
            this.mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.AddListener(this.logMessageJsonPoster, null)
            );

        [Test]
        public void Should_PostJson_LogMessage_When_Receives()
        {
            var logMessage = new LogMessage();
            this.logMessageJsonPoster.Handle(logMessage);

            this.mockJsonPoster.Verify(
                jsonPoster => jsonPoster.PostJson(this.logMessageJsonPoster.Type, logMessage)
            );
        }

        [Test]
        public void Should_Not_Repost_When_Refresh()
        {
            var logMessage = new LogMessage();
            this.logMessageJsonPoster.Handle(logMessage);
            this.mockJsonPoster.Reset();

            this.logMessageJsonPoster.Refresh();

            this.mockJsonPoster.VerifyNoOtherCalls();
        }

        [Test]
        public void Should_Have_NotReadyPostBehaviour_As_KeepAll() =>
            Assert.That(this.logMessageJsonPoster.NotReadyPostBehaviour, Is.EqualTo(NotReadyPostBehaviour.KeepAll));

    }
}
