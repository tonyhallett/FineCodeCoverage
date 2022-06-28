namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.WebView;
    using NUnit.Framework;

    [SetUpFixture]
    internal class InstallWebViewRuntime
    {
        [OneTimeSetUp]
        public Task EnsureInstalledAsync()
        {
            var webViewRuntime = new WebViewRuntime(
                new WebViewRuntimeInstallationChecker(new WebViewRuntimeRegistry()),
                new WebViewRuntimeInstaller(new ProcessUtil(), new EnvironmentWrapper())
            );

            return webViewRuntime.InitializeAsync(true, CancellationToken.None);
        }
    }
}

