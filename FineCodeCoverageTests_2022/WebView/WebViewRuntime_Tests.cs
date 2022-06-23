namespace FineCodeCoverageTests.WebView_Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Output.WebView;
    using NUnit.Framework;

    internal class WebViewRuntime_Tests
    {
        private AutoMoqer mocker;
        private WebViewRuntime webViewRuntime;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.webViewRuntime = this.mocker.Create<WebViewRuntime>();
        }

        [Test]
        public void Should_Initially_Not_Be_Installed() =>
            Assert.That(this.webViewRuntime.IsInstalled, Is.False);

        private Task InitializeInstalledAsync()
        {
            _ = this.mocker.GetMock<IWebViewRuntimeInstallationChecker>()
                .Setup(webViewRuntimeInstallationChecker => webViewRuntimeInstallationChecker.IsInstalled())
                .Returns(true);

            return this.webViewRuntime.InitializeAsync(CancellationToken.None);
        }

        [Test]
        public async Task Should_Raise_Installed_Event_When_Initialize_And_Is_Already_Installed_Async()
        {
            var raisedInstalledEvent = false;
            this.webViewRuntime.Installed += (sender, args) => raisedInstalledEvent = true;

            await this.InitializeInstalledAsync();

            Assert.That(raisedInstalledEvent, Is.True);

        }

        [Test]
        public async Task Should_Be_Installed_When_Initialize_And_Is_Already_Installed_Async()
        {
            await this.InitializeInstalledAsync();

            Assert.That(this.webViewRuntime.IsInstalled, Is.True);
        }

        private async Task<CancellationToken> InitializeNotInstalledAsync()
        {
            var cancellationToken = CancellationToken.None;
            await this.webViewRuntime.InitializeAsync(cancellationToken);
            return cancellationToken;
        }

        [Test]
        public async Task Should_Install_When_Initialize_And_Is_Not_Installed_Async()
        {
            var cancellationToken = await this.InitializeNotInstalledAsync();

            this.mocker.Verify<IWebViewRuntimeInstaller>(webViewRuntimeInstaller => webViewRuntimeInstaller.InstallAsync(cancellationToken));
        }

        [Test]
        public async Task Should_Be_Installed_After_Installer_Installs_Async()
        {
            _ = await this.InitializeNotInstalledAsync();

            Assert.That(this.webViewRuntime.IsInstalled, Is.True);
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
