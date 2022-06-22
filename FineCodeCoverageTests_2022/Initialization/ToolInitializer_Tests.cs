namespace FineCodeCoverageTests.Initialization_Tests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Core.Initialization.ZippedTools;
    using NUnit.Framework;

    public class ToolInitializer_Tests
    {
        private class TestRequireToolUnzipping : IRequireToolUnzipping
        {
            public TestRequireToolUnzipping(string zipDirectoryName, string zipPrefix)
            {
                this.ZipDirectoryName = zipDirectoryName;
                this.ZipPrefix = zipPrefix;
            }
            public string ZipDirectoryName { get; }

            public string ZipPrefix { get; }

            public string ZipDestination { get; set; }

            public void SetZipDestination(string zipDestination) => this.ZipDestination = zipDestination;
        }

        [Test]
        public async Task Should_Ensure_Unzipped_For_All_IRequireToolUnzipping_Setting_Zip_Destination_Async()
        {
            var mocker = new AutoMoqer();
            var requireToolUnzipping1 = new TestRequireToolUnzipping("Dir1", "Prefix1");
            var requireToolUnzipping2 = new TestRequireToolUnzipping("Dir2", "Prefix2");
            mocker.SetInstance<IEnumerable<IRequireToolUnzipping>>(new List<IRequireToolUnzipping>
            {
                requireToolUnzipping1, requireToolUnzipping2
            });

            _ = mocker.GetMock<IAppDataFolder>().Setup(appDataFolder => appDataFolder.GetDirectoryPath())
                .Returns("FCCAppDataFolder");

            var cancellationToken = CancellationToken.None;
            var mockToolZipProvider = mocker.GetMock<IToolZipProvider>();
            var zipDetails1 = new ZipDetails
            {
                Path = "Path1"
            };
            var zipDetails2 = new ZipDetails
            {
                Path = "Path2"
            };
            _ = mockToolZipProvider.Setup(toolZipProvider => toolZipProvider.ProvideZip("Prefix1")).Returns(zipDetails1);
            _ = mockToolZipProvider.Setup(toolZipProvider => toolZipProvider.ProvideZip("Prefix2")).Returns(zipDetails2);

            _ = mocker.GetMock<IToolFolder>().Setup(
                toolFolder => toolFolder.EnsureUnzipped("FCCAppDataFolder", "Dir1", zipDetails1, cancellationToken)
            ).Returns("ZipDestination1");
            _ = mocker.GetMock<IToolFolder>().Setup(
                toolFolder => toolFolder.EnsureUnzipped("FCCAppDataFolder", "Dir2", zipDetails2, cancellationToken)
            ).Returns("ZipDestination2");

            var toolInitializer = mocker.Create<ToolInitializer>();

            await toolInitializer.InitializeAsync(cancellationToken);

            Assert.Multiple(() =>
            {
                Assert.That(requireToolUnzipping1.ZipDestination, Is.EqualTo("ZipDestination1"));
                Assert.That(requireToolUnzipping2.ZipDestination, Is.EqualTo("ZipDestination2"));
            });
        }
    }

}
