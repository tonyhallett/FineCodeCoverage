namespace FineCodeCoverageTests.WebView_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.JsMessages;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.JsSerialization.ReportGenerator;
    using Moq;
    using NUnit.Framework;
    using Palmmedia.ReportGenerator.Core.CodeAnalysis;
    using Palmmedia.ReportGenerator.Core.Parser.Analysis;

    internal class ReportJsonPoster_Tests
    {
        private AutoMoqer mocker;
        private ReportJsonPoster reportJsonPoster;
        private Mock<IJsonPoster> mockJsonPoster;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();

            this.reportJsonPoster = this.mocker.Create<ReportJsonPoster>();

            this.mockJsonPoster = new Mock<IJsonPoster>();
            this.reportJsonPoster.Ready(this.mockJsonPoster.Object, null);
        }

        [Test]
        public void Should_Add_Itself_As_Message_Listener() =>
            this.mocker.Verify<IEventAggregator>(eventAggregator => eventAggregator.AddListener(this.reportJsonPoster, null));


        private IReport SendNewReportMessage()
        {
            var newReportMessage = new NewReportMessage
            {
                RiskHotspotAnalysisResult = new RiskHotspotAnalysisResult(EmptyReadOnlyCollection.Of<RiskHotspot>(), false),
                RiskHotspotsAnalysisThresholds = new RiskHotspotsAnalysisThresholds(),
                SummaryResult = new SummaryResult(
                    EmptyReadOnlyCollection.Of<Assembly>(),
                    "",
                    false,
                    EmptyReadOnlyCollection.Of<string>()
                    )
            };
            var report = new Mock<IReport>().Object;
            _ = this.mocker.GetMock<IReportFactory>().Setup(
                reportFactory => reportFactory.Create(
                    newReportMessage.RiskHotspotAnalysisResult,
                    newReportMessage.RiskHotspotsAnalysisThresholds,
                    newReportMessage.SummaryResult
                )
            ).Returns(report);

            this.reportJsonPoster.Handle(newReportMessage);

            return report;
        }

        [Test]
        public void Should_Post_Report_When_Receives_NewReportMessage()
        {
            var expectedReport = this.SendNewReportMessage();

            this.mockJsonPoster.Verify(jsonPoster => jsonPoster.PostJson("report", expectedReport));
        }

        [Test]
        public void Should_Post_Null_Report_When_Receives_ClearReportMessage()
        {
            _ = this.SendNewReportMessage();

            this.reportJsonPoster.Handle(new ClearReportMessage());

            this.mockJsonPoster.Verify(jsonPoster => jsonPoster.PostJson<IReport>("report", null));
        }

        [Test]
        public void Should_Repost_When_Refresh()
        {
            _ = this.SendNewReportMessage();

            this.reportJsonPoster.Refresh();

            this.mockJsonPoster.AssertReInvokes();
        }
    }




}
