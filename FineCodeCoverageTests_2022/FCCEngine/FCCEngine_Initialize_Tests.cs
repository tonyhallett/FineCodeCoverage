namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System.Collections.Generic;
    using System.Threading;
    using AutoMoq;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.MsTestPlatform;
    using NUnit.Framework;

    public class FCCEngine_Initialize_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.fccEngine = this.mocker.Create<FCCEngine>();
        }

        [Test]
        public void Should_Initialize_AppFolder_Then_Utils()
        {
            var disposalToken = CancellationToken.None;
            var callOrder = new List<int>();

            var appDataFolderPath = "some path";
            var mockAppDataFolder = this.mocker.GetMock<IAppDataFolder>();
            _ = mockAppDataFolder.Setup(appDataFolder => appDataFolder.Initialize(disposalToken)).Callback(() => callOrder.Add(1));
            _ = mockAppDataFolder.Setup(appDataFolder => appDataFolder.DirectoryPath).Returns(appDataFolderPath);

            var msTestPlatformMock = this.mocker.GetMock<IMsTestPlatformUtil>().Setup(msTestPlatform => msTestPlatform.Initialize(appDataFolderPath, disposalToken)).Callback(() => callOrder.Add(2));

            var openCoverMock = this.mocker.GetMock<ICoverageUtilManager>().Setup(openCover => openCover.Initialize(appDataFolderPath, disposalToken)).Callback(() => callOrder.Add(3));

            this.fccEngine.Initialize(null, disposalToken);

            Assert.That(callOrder, Has.Count.EqualTo(3));
            Assert.That(callOrder[0], Is.EqualTo(1));
        }

        [Test]
        public void Should_Set_AppDataFolderPath_From_Initialized_AppDataFolder_DirectoryPath()
        {
            var appDataFolderPath = "some path";
            var mockAppDataFolder = this.mocker.GetMock<IAppDataFolder>();
            _ = mockAppDataFolder.Setup(appDataFolder => appDataFolder.DirectoryPath).Returns(appDataFolderPath);
            this.fccEngine.Initialize(null, CancellationToken.None);
            Assert.That(this.fccEngine.AppDataFolderPath, Is.EqualTo("some path"));
        }

    }

}
