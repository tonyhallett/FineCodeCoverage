namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Threading.Tasks;
    using FineCodeCoverageWebViewReport_Tests.Installation;
    using NUnit.Framework;

    [SetUpFixture]
    internal class InstallRequisites
    {
        [OneTimeSetUp]
        public async Task EnsureInstalledAsync()
        {
            await AppiumInstaller.EnsureGloballyInstalledWithNpmAsync();
            await new WebViewRuntimeApplicableVersion().EnsureApplicableVersionInstalledAsync();
        }
    }
}

