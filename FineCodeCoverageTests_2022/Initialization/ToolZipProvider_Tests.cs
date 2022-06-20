namespace FineCodeCoverageTests.Initialization_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization.ZippedTools;
    using FineCodeCoverage.Core.Utilities;
    using Moq;
    using NUnit.Framework;

    public class ToolFolder_Tests
    {
        private AutoMoqer mocker;

        private ToolFolder toolFolder;

        private readonly string expectedZipDestination = Path.Combine("AppDataFolder", "Tool", "1.0.0");
        private readonly string expectedToolDirectory = Path.Combine("AppDataFolder", "Tool");
        private readonly string zipFilePath = "ZipFilePath";

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.toolFolder = this.mocker.Create<ToolFolder>();
        }

        private string Unzip() => this.toolFolder.EnsureUnzipped(
                "AppDataFolder",
                "Tool",
                new ZipDetails { Version = "1.0.0", Path =  zipFilePath},
                CancellationToken.None
            );

        [Test]
        public void Should_Have_Zip_Destination_As_Version_In_Tool_Folder_In_AppData_Folder()
        {
            var zipDestination = this.Unzip();

            Assert.That(zipDestination, Is.EqualTo(this.expectedZipDestination));
        }

        [Test]
        public void Should_Do_Nothing_If_The_Versioned_Zip_Exists_In_The_App_Data_Folder_Tool_Folder()
        {
            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(fileUtil => fileUtil.Exists(this.expectedZipDestination)).Returns(true);

            _ = this.Unzip();

            mockFileUtil.VerifyAll();
            mockFileUtil.VerifyNoOtherCalls();

            this.mocker.GetMock<IZipFile>().VerifyNoOtherCalls();
        }

        [Test]
        public void Should_Try_Delete_Old_Versions_Before_Unzipping_New_Version_Tool_Directory_Exists()
        {
            var unzipped = false;
            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryExists(this.expectedToolDirectory)).Returns(true);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.TryDeleteDirectory(this.expectedToolDirectory))
                .Callback(() => Assert.That(unzipped, Is.False));
            _ = this.mocker.Setup<IZipFile>(
                zipFile => zipFile.ExtractToDirectory(this.zipFilePath, this.expectedZipDestination)
            ).Callback(() => unzipped = true);

            _ = this.Unzip();

            mockFileUtil.VerifyAll();
            Assert.That(unzipped, Is.True);
        }

        [Test]
        public void Should_Not_Try_Delete_Old_Versions_If_Tool_Directory_Does_Not_Exist()
        {
            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryExists(this.expectedToolDirectory)).Returns(false);

            _ = this.Unzip();

            mockFileUtil.Verify(fileUtil => fileUtil.TryDeleteDirectory(this.expectedToolDirectory), Times.Never());
        }
    }

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
