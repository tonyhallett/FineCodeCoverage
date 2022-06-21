namespace FineCodeCoverageTests.Initialization_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization.ZippedTools;
    using FineCodeCoverage.Core.Utilities;
    using Moq;
    using NUnit.Framework;

    public class ToolZipProvider_Tests
    {
        private AutoMoqer mocker;
        private Mock<IFileUtil> mockFileUtil;
        private ToolZipProvider toolZipProvider;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mockFileUtil = this.mocker.GetMock<IFileUtil>();
            this.toolZipProvider = this.mocker.Create<ToolZipProvider>();
        }

        private ZipDetails ProvideZip(IEnumerable<string> zipFiles)
        {
            var extensionDirectory = Path.GetDirectoryName(typeof(IFileUtil).Assembly.Location);
            var expectedSearchFolder = Path.Combine(extensionDirectory, "ZippedTools");
            _ = this.mockFileUtil.Setup(fileUtil => fileUtil.DirectoryGetFiles(expectedSearchFolder, $"sometool.*.zip"))
                .Returns(zipFiles);

            return this.toolZipProvider.ProvideZip("sometool");
        }

        [Test]
        public void Should_Search_For_Zips_In_The_Extension_Directory_ZippedTools_Directory()
        {
            var zipPath = "SomePath\\sometool.1.zip";

            var zipDetails = this.ProvideZip(new List<string> { zipPath });

            Assert.Multiple(() =>
            {
                Assert.That(zipDetails.Version, Is.EqualTo("1"));
                Assert.That(zipDetails.Path, Is.EqualTo(zipPath));
            });

        }

        [Test]
        public void Should_Throw_If_No_Matching_Zip() =>
            _ = Assert.Throws<InvalidOperationException>(() => this.ProvideZip(Enumerable.Empty<string>()));

        [Test]
        public void Should_Throw_If_Multiple_Matching_Zips() =>
            _ = Assert.Throws<InvalidOperationException>(() => this.ProvideZip(new List<string> { "first", "second" }));

    }

}
