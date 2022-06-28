namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Output.WebView;
    using NUnit.Framework;

    [TestFixture(true)]
    [TestFixture(false)]
    internal class WebViewRuntime_Tests
    {
        private AutoMoqer mocker;
        private WebViewRuntime webViewRuntime;
        private readonly bool testExplorerInstantiation;

        public WebViewRuntime_Tests(bool testExplorerInstantiation) =>
            this.testExplorerInstantiation = testExplorerInstantiation;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.webViewRuntime = this.mocker.Create<WebViewRuntime>();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Installed_When_WebViewRuntimeInstallationChecker_IsInstalled(bool isInstalled)
        {
            _ = this.mocker.GetMock<IWebViewRuntimeInstallationChecker>()
                .Setup(checker => checker.IsInstalled()).Returns(isInstalled);

            Assert.That(this.webViewRuntime.IsInstalled, Is.EqualTo(isInstalled));

        }

        private async Task<CancellationToken> InitializeNotInstalledAsync()
        {
            var cancellationToken = CancellationToken.None;
            await this.webViewRuntime.InitializeAsync(this.testExplorerInstantiation, cancellationToken);
            return cancellationToken;
        }

        [Test]
        public async Task Should_Install_Without_UI_When_Initialize_And_Is_Not_Installed_Async()
        {
            var cancellationToken = await this.InitializeNotInstalledAsync();

            this.mocker.Verify<IWebViewRuntimeInstaller>(
                webViewRuntimeInstaller => webViewRuntimeInstaller.InstallAsync(cancellationToken, true)
            );
        }

        [Test]
        public async Task Should_Raise_Installed_After_Installer_Installs_Async()
        {
            var raisedInstalledEvent = false;
            this.webViewRuntime.Installed += (sender, args) => raisedInstalledEvent = true;

            _ = await this.InitializeNotInstalledAsync();

            Assert.That(raisedInstalledEvent, Is.True);
        }
    }
}
