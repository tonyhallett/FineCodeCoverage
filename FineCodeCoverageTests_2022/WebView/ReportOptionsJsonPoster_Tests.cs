namespace FineCodeCoverageTests.WebView_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Output.JsPosting;
    using Moq;
    using NUnit.Framework;

    internal class ReportOptionsJsonPoster_Tests
    {
        private ReportOptionsJsonPoster reportOptionsJsonPoster;
        private Mock<IJsonPoster> mockJsonPoster;
        private ReportOptions reportOptions;
        private Mock<IReportOptionsProvider> mockReportOptionsProvider;

        [SetUp]
        public void SetUp()
        {
            var mocker = new AutoMoqer();
            this.reportOptions = new ReportOptions();
            this.mockReportOptionsProvider = mocker.GetMock<IReportOptionsProvider>();
            _ = this.mockReportOptionsProvider.Setup(reportOptionsProvider => reportOptionsProvider.Provide())
                .Returns(this.reportOptions);

            this.reportOptionsJsonPoster = mocker.Create<ReportOptionsJsonPoster>();

            this.mockJsonPoster = new Mock<IJsonPoster>();

            this.reportOptionsJsonPoster.Initialize(this.mockJsonPoster.Object);
        }

        private void VerifyPostsReportOptions(ReportOptions reportOptions, Times times) =>
            this.mockJsonPoster.Verify(
                jsonPoster => jsonPoster.PostJson(
                    this.reportOptionsJsonPoster.Type,
                    Parameter.Is<ReportOptions>().That(Is.SameAs(reportOptions))
                ),
                times
            );

        [Test]
        public void Should_Have_NotReadyPostBehaviour_As_KeepLast() =>
            Assert.That(this.reportOptionsJsonPoster.NotReadyPostBehaviour, Is.EqualTo(NotReadyPostBehaviour.KeepLast));

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Post_ReportOptions_From_The_ReportOptionsProvider_When_Ready(bool ready)
        {
            if (ready)
            {
                this.reportOptionsJsonPoster.Ready(null);
            }
            var expectedTimes = ready ? Times.Once() : Times.Never();
            this.VerifyPostsReportOptions(this.reportOptions, expectedTimes);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Post_ReportOptions_When_ReportOptions_Change_When_Ready(bool ready)
        {
            if (ready)
            {
                this.reportOptionsJsonPoster.Ready(null);
            }

            var newReportOptions = new ReportOptions();
            this.mockReportOptionsProvider.Raise(
                reportOptionsProvider => reportOptionsProvider.ReportOptionsChanged += null,
                this.mockReportOptionsProvider.Object,
                newReportOptions
            );

            var expectedTimes = ready ? Times.Once() : Times.Never();
            this.VerifyPostsReportOptions(newReportOptions, expectedTimes);
        }

        [Test]
        public void Should_Repost_When_Refresh()
        {
            this.reportOptionsJsonPoster.Ready(null);

            this.reportOptionsJsonPoster.Refresh();

            this.mockJsonPoster.AssertReInvokes();
        }
    }
}
