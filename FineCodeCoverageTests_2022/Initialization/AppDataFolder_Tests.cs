namespace FineCodeCoverageTests.Initialization_Tests
{
    using System;
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    internal class AppDataFolder_Tests
    {
        private AutoMoqer mocker;
        private AppDataFolder appDataFolder;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.appDataFolder = this.mocker.Create<AppDataFolder>();
        }

        private void SetUpToolsDirectoryOption(string toolsDirectory)
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(appOptions => appOptions.ToolsDirectory).Returns(toolsDirectory);
            _ = this.mocker.GetMock<IAppOptionsProvider>()
                .Setup(appOptionsProvider => appOptionsProvider.Provide())
                .Returns(mockAppOptions.Object);
        }

        [Test]
        public void Should_Create_If_Does_Not_Exist_FineCodeCoverage_Directory_In_ToolsDirectory_From_Options_If_Provided()
        {
            this.SetUpToolsDirectoryOption("ToolsDirectory");

            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryExists("ToolsDirectory"))
                .Returns(true);

            var expectedFCCDirectory = Path.Combine("ToolsDirectory", "FineCodeCoverage");

            var directoryPath = this.appDataFolder.GetDirectoryPath();

            Assert.That(directoryPath, Is.EqualTo(expectedFCCDirectory));
            mockFileUtil.Verify(fileUtil => fileUtil.CreateDirectory(expectedFCCDirectory));

        }

        [Test]
        public void Should_Default_Create_If_Does_Not_Exist_FineCodeCoverage_Directory_In_Local_App_Data()
        {
            this.SetUpToolsDirectoryOption(null);

            var expectedFCCDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FineCodeCoverage");

            var directoryPath = this.appDataFolder.GetDirectoryPath();

            Assert.That(directoryPath, Is.EqualTo(expectedFCCDirectory));
            this.mocker.Verify<IFileUtil>(fileUtil => fileUtil.CreateDirectory(expectedFCCDirectory));
        }

        private void SetUpFCCDebugCleanInstall()
        {
            this.SetUpToolsDirectoryOption(null);
            _ = this.mocker.GetMock<IEnvironmentVariable>()
                .Setup(environmentVariable => environmentVariable.Get("FCCDebugCleanInstall"))
                .Returns("Yes");
        }

        [Test]
        public void Should_Log_FCC_Clean_Install_When_Environment_Variable_FCCDebugCleanInstall()
        {
            this.SetUpFCCDebugCleanInstall();

            _ = this.appDataFolder.GetDirectoryPath();

            this.mocker.Verify<ILogger>(logger => logger.Log("FCC Clean Install"));
        }

        [Test]
        public void Should_Log_FCC_App_Data_Folder_Does_Not_Exist_When_Environment_Variable_FCCDebugCleanInstall_And_Does_Not_Exist()
        {
            this.SetUpFCCDebugCleanInstall();

            _ = this.appDataFolder.GetDirectoryPath();

            this.mocker.Verify<ILogger>(logger => logger.Log("FCC App data folder does not exist"));
        }

        [Test]
        public void Should_Try_Delete_Existing_FCC_Directory_When_Environment_Variable_FCCDebugCleanInstall()
        {
            this.SetUpFCCDebugCleanInstall();

            var expectedFCCDirectory = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
               "FineCodeCoverage");

            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryExists(expectedFCCDirectory)).Returns(true);

            _ = this.appDataFolder.GetDirectoryPath();

            mockFileUtil.Verify(fileUtil => fileUtil.TryDeleteDirectory(expectedFCCDirectory));
        }

        [TestCase(true, "Deleted FCC app data folder")]
        [TestCase(false, "Error deleting FCC app data folder")]
        public void Should_Log_Deleted_FCC_App_Data_Folder_When_Environment_Variable_FCCDebugCleanInstall_And_Deletes(bool successfullyDeletes, string expectedLogMessage)
        {
            this.SetUpFCCDebugCleanInstall();

            var expectedFCCDirectory = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
               "FineCodeCoverage");

            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryExists(expectedFCCDirectory)).Returns(true);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.TryDeleteDirectory(expectedFCCDirectory)).Returns(successfullyDeletes);

            _ = this.appDataFolder.GetDirectoryPath();

            this.mocker.Verify<ILogger>(logger => logger.Log(expectedLogMessage));
        }

    }
}
