namespace FineCodeCoverageTests.Tools_Test
{
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using NUnit.Framework;

    public class ToolZipProvider_Tests
    {
        private DirectoryInfo extensionDirectory;
        private DirectoryInfo zippedToolsDirectory;

        [SetUp]
        public void CreateCoverageOutputFolder()
        {
            this.extensionDirectory = this.CreateTemporaryDirectory();
            this.zippedToolsDirectory = this.extensionDirectory.CreateSubdirectory(ToolZipProvider.ZippedToolsDirectoryName);
        }

        [TearDown]
        public void DeleteCoverageOutputFolder() => this.extensionDirectory.Delete(true);

        private DirectoryInfo CreateTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Directory.CreateDirectory(tempDirectory);

        }

        [TestCase("1.0.0")]
        [TestCase("3.0.0")]
        public void Should_Provide_Zip_In_Extension_Directory_By_Naming_Convention(string version)
        {
            var zipPrefix = "zipPrefix";
            var zipPath = Path.Combine(this.zippedToolsDirectory.FullName, $"{zipPrefix}.{version}.zip");
            File.WriteAllText(zipPath, "");
            var mocker = new AutoMoqer();
            var zipProvider = mocker.Create<ToolZipProvider>();
            zipProvider.ExtensionDirectory = this.extensionDirectory.FullName;

            var zipDetails = zipProvider.ProvideZip(zipPrefix);

            Assert.Multiple(() =>
            {
                Assert.That(zipDetails.Path, Is.EqualTo(zipPath));
                Assert.That(zipDetails.Version, Is.EqualTo(version));
            });
        }
    }
}
