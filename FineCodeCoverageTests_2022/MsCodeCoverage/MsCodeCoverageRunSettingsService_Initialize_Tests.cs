namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System.Threading;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using NUnit.Framework;

    internal class MsCodeCoverageRunSettingsService_Initialize_Tests
    {
        [Test]
        public void Should_Ensure_Microsoft_CodeCoverage_Is_Unzipped_To_The_Tool_Folder()
        {
            var autoMocker = new AutoMoqer();
            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();

            var cancellationToken = CancellationToken.None;

            var zipDetails = new ZipDetails();
            var mockToolZipProvider = autoMocker.GetMock<IToolZipProvider>();
            _ = mockToolZipProvider.Setup(toolZipProvider => toolZipProvider.ProvideZip("microsoft.codecoverage")).Returns(zipDetails);

            var mockToolFolder = autoMocker.GetMock<IToolFolder>();
            _ = mockToolFolder.Setup(toolFolder =>
                  toolFolder.EnsureUnzipped("AppDataFolder", "msCodeCoverage", zipDetails, cancellationToken)).Returns("ZipDestination");

            msCodeCoverageRunSettingsService.Initialize("AppDataFolder", null, cancellationToken);
            mockToolFolder.VerifyAll();
        }
    }
}
