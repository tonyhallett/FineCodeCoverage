namespace FineCodeCoverageTests.WebView_Tests
{
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.WebView;
    using NUnit.Framework;

    internal class ReportPathsProvider_Tests
    {
        private AutoMoqer mocker;
        private ReportPathsProvider reportPathsProvider;
        private string expectedFCCNavigationPath;

        [SetUp]
        public void Setup()
        {
            this.mocker = new AutoMoqer();
            this.reportPathsProvider = this.mocker.Create<ReportPathsProvider>();

            this.expectedFCCNavigationPath = Path.Combine(FCCExtension.Directory, "Resources", "index.html");
        }

        [Test]
        public void Should_Cache_Provided_ReportPaths()
        {
            var first = this.reportPathsProvider.Provide();
            var second = this.reportPathsProvider.Provide();
            Assert.That(first, Is.SameAs(second));
        }

        #region fcc provided

        private void Assert_Is_Path_From_Extension_Resources(string path) =>
            Assert.That(path, Is.EqualTo(this.expectedFCCNavigationPath));

        [Test]
        public void Should_Provide_Standalone_Path_From_Extension_Resources()
        {
            this.reportPathsProvider.debug = false;
            this.Assert_Is_Path_From_Extension_Resources(this.reportPathsProvider.Provide().StandalonePath);
        }

        [Test]
        public void Should_Provide_Navigation_Path_From_Extension_Resources_When_Release_Build()
        {
            this.reportPathsProvider.debug = false;
            this.Assert_Is_Path_From_Extension_Resources(this.reportPathsProvider.Provide().NavigationPath);
        }

        [Test]
        public void Should_Provide_Navigation_Path_From_PreBuild_Debug_Dist_When_Debug_Build() =>
            Assert.That(this.reportPathsProvider.Provide().NavigationPath, Is.EqualTo(DebugReportPath.Path));

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Watch_When_Debug_Build(bool debug)
        {
            this.reportPathsProvider.debug = debug;
            Assert.That(this.reportPathsProvider.Provide().ShouldWatch, Is.EqualTo(debug));
        }
        #endregion
    }
}
