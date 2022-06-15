namespace FineCodeCoverageTests.WebView_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.JsMessages;
    using FineCodeCoverage.Output.JsPosting;
    using Moq;
    using NUnit.Framework;

    internal class CoverageStoppedPoster_Tests {
        private AutoMoqer mocker;
        private CoverageStoppedJsonPoster coverageStoppedJsonPoster;
        private Mock<IJsonPoster> mockJsonPoster;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.coverageStoppedJsonPoster = this.mocker.Create<CoverageStoppedJsonPoster>();

            this.mockJsonPoster = new Mock<IJsonPoster>();
            this.coverageStoppedJsonPoster.Ready(this.mockJsonPoster.Object, null);
        }

        [Test]
        public void Should_Add_Itself_As_Listener_For_CoverageStoppedMessage() =>
            this.mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.AddListener(this.coverageStoppedJsonPoster, null)
            );

        [Test]
        public void Should_Post_When_Received_Should_Add_Itself_As_Listener_For_CoverageStoppedMessage()
        {
            this.coverageStoppedJsonPoster.Handle(new CoverageStoppedMessage());

            this.mockJsonPoster.Verify(jsonPoster => jsonPoster.PostJson<object>("coverageStopped", null));
        }

        [Test]
        public void Should_Not_Repost_When_Refresh()
        {
            this.coverageStoppedJsonPoster.Refresh();

            this.mockJsonPoster.VerifyNoOtherCalls();
        }
    }




}
