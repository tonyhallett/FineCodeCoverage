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

        private void VerifyPostsReportOptions(ReportOptions reportOptions) =>
            this.mockJsonPoster.Verify(
                jsonPoster => jsonPoster.PostJson(
                    this.reportOptionsJsonPoster.Type,
                    Parameter.Is<ReportOptions>().That(Is.SameAs(reportOptions))
                )
            );

        [Test]
        public void Should_Have_NotReadyPostBehaviour_As_KeepLast() =>
            Assert.That(this.reportOptionsJsonPoster.NotReadyPostBehaviour, Is.EqualTo(NotReadyPostBehaviour.KeepLast));

        [Test]
        public void Should_Post_ReportOptions_From_The_ReportOptionsProvider() =>
            this.VerifyPostsReportOptions(this.reportOptions);

        [Test]
        public void Should_Post_ReportOptions_When_ReportOptions_Change()
        {
            var newReportOptions = new ReportOptions();
            this.mockReportOptionsProvider.Raise(
                reportOptionsProvider => reportOptionsProvider.ReportOptionsChanged += null,
                this.mockReportOptionsProvider.Object,
                newReportOptions
            );

            this.VerifyPostsReportOptions(newReportOptions);
        }

        [Test]
        public void Should_Repost_When_Refresh()
        {
            this.reportOptionsJsonPoster.Refresh();

            this.mockJsonPoster.AssertReInvokes();
        }
    }
}
