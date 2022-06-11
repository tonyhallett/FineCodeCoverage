using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.MsTestPlatform;
using NUnit.Framework;

namespace FineCodeCoverageTests.FCCEngine_Tests
{
    public class FCCEngine_Initialize_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            fccEngine = mocker.Create<FCCEngine>();
        }

        [Test]
        public void Should_Initialize_AppFolder_Then_Utils()
        {
            var disposalToken = CancellationToken.None;
            List<int> callOrder = new List<int>();

            var appDataFolderPath = "some path";
            var mockAppDataFolder = mocker.GetMock<IAppDataFolder>();
            mockAppDataFolder.Setup(appDataFolder => appDataFolder.Initialize(disposalToken)).Callback(() => callOrder.Add(1));
            mockAppDataFolder.Setup(appDataFolder => appDataFolder.DirectoryPath).Returns(appDataFolderPath);


            var msTestPlatformMock = mocker.GetMock<IMsTestPlatformUtil>().Setup(msTestPlatform => msTestPlatform.Initialize(appDataFolderPath, disposalToken)).Callback(() => callOrder.Add(2));

            var openCoverMock = mocker.GetMock<ICoverageUtilManager>().Setup(openCover => openCover.Initialize(appDataFolderPath, disposalToken)).Callback(() => callOrder.Add(3));

            fccEngine.Initialize(null,disposalToken);

            Assert.AreEqual(3, callOrder.Count);
            Assert.AreEqual(1, callOrder[0]);
        }
        
        [Test]
        public void Should_Set_AppDataFolderPath_From_Initialized_AppDataFolder_DirectoryPath()
        {
            var appDataFolderPath = "some path";
            var mockAppDataFolder = mocker.GetMock<IAppDataFolder>();
            mockAppDataFolder.Setup(appDataFolder => appDataFolder.DirectoryPath).Returns(appDataFolderPath);
            fccEngine.Initialize(null, CancellationToken.None);
            Assert.AreEqual("some path", fccEngine.AppDataFolderPath);
        }

    }

}