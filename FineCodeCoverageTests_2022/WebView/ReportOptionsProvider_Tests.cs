namespace FineCodeCoverageTests.WebView_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output.JsPosting;
    using Moq;
    using NUnit.Framework;

    internal class ReportOptionsProvider_Tests
    {
        private ReportOptionsProvider reportOptionsProvider;
        private Mock<IAppOptionsProvider> mockAppOptionsProvider;

        [SetUp]
        public void SetUp()
        {
            var mocker = new AutoMoqer();
            this.mockAppOptionsProvider = mocker.GetMock<IAppOptionsProvider>();
            var appOptions = this.CreateProvidedAppOptions();
            _ = this.mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Provide()).Returns(appOptions);

            this.reportOptionsProvider = mocker.Create<ReportOptionsProvider>();
        }

        private IAppOptions CreateAppOptions(bool hideFullyCovered, bool namespacedClasses)
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(appOptions => appOptions.HideFullyCovered).Returns(hideFullyCovered);
            _ = mockAppOptions.SetupGet(appOptions => appOptions.NamespacedClasses).Returns(namespacedClasses);

            return mockAppOptions.Object;
        }

        private IAppOptions CreateProvidedAppOptions() => this.CreateAppOptions(true, true);

        [Test]
        public void Should_Provide_Report_Options_From_AppOptions()
        {
            var reportOptions = this.reportOptionsProvider.Provide();

            Assert.Multiple(() =>
            {
                Assert.That(reportOptions.hideFullyCovered, Is.True);
                Assert.That(reportOptions.namespacedClasses, Is.True);
            });
        }

        [Test]
        public void Should_Provide_Report_Options_From_The_Latest_AppOptions()
        {
            var changedAppOptions = this.CreateAppOptions(true, false);
            this.mockAppOptionsProvider.Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, changedAppOptions);

            var reportOptions = this.reportOptionsProvider.Provide();

            Assert.Multiple(() =>
            {
                Assert.That(reportOptions.hideFullyCovered, Is.True);
                Assert.That(reportOptions.namespacedClasses, Is.False);
            });
        }

        [Test]
        public void Should_Raise_ReportOptionsChange_When_They_Change()
        {
            _ = this.reportOptionsProvider.Provide();

            ReportOptions raisedReportOptions = null;
            this.reportOptionsProvider.ReportOptionsChanged += (sender, changedReportOptions) =>
                raisedReportOptions = changedReportOptions;

            var changedAppOptions = this.CreateAppOptions(true, false);
            this.mockAppOptionsProvider.Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, changedAppOptions);

            Assert.Multiple(() =>
            {
                Assert.That(raisedReportOptions.hideFullyCovered, Is.True);
                Assert.That(raisedReportOptions.namespacedClasses, Is.False);
            });
        }

        [Test]
        public void Should_Not_Raise_ReportOptionsChange_When_They_Do_Not()
        {
            _ = this.reportOptionsProvider.Provide();

            ReportOptions raisedReportOptions = null;
            this.reportOptionsProvider.ReportOptionsChanged += (sender, changedReportOptions) =>
                raisedReportOptions = changedReportOptions;

            var changedAppOptions = this.CreateProvidedAppOptions();
            this.mockAppOptionsProvider.Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, changedAppOptions);

            Assert.That(raisedReportOptions, Is.Null);
        }
    }




}
