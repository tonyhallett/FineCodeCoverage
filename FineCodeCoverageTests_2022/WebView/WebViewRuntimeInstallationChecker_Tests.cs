namespace FineCodeCoverageTests.WebView_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class WebViewRuntimeInstallationChecker_Tests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Installed_If_Entries_In_The_Registry(bool entriesInRegistry)
        {
            var mocker = new AutoMoqer();
            var webViewRuntimeInstallationChecker = mocker.Create<WebViewRuntimeInstallationChecker>();
            var entries = entriesInRegistry ? new Mock<IWebViewRuntimeRegistryEntries>().Object : null;
            mocker.GetMock<IWebViewRuntimeRegistry>().Setup(webViewRuntimeRegistry => webViewRuntimeRegistry.GetEntries()).Returns(entries);

            Assert.That(webViewRuntimeInstallationChecker.IsInstalled, Is.EqualTo(entriesInRegistry));
        }
    }
}
