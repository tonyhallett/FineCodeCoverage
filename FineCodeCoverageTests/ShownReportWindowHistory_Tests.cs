using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using Moq;
using NUnit.Framework;
using System.IO;

namespace FineCodeCoverageTests
{
    internal class ShownReportWindowHistory_Tests
    {
        private AutoMoqer mocker;
        private ShownReportWindowHistory shownReportWindowHistory;
        private string markerFilePath;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            shownReportWindowHistory = mocker.Create<ShownReportWindowHistory>();
            mocker.GetMock<IFCCEngine>().Setup(fccEngine => fccEngine.AppDataFolderPath).Returns("AppDataFolderPath");
            markerFilePath = Path.Combine("AppDataFolderPath", "outputWindowInitialized");
        }

        [Test]
        public void It_Should_Write_Marker_File_When_ShowedToolWindow_First_Time()
        {
            shownReportWindowHistory.Showed();
            mocker.Verify<IFileUtil>(f => f.WriteAllText(markerFilePath, string.Empty));
            shownReportWindowHistory.Showed();
            mocker.Verify<IFileUtil>(f => f.WriteAllText(markerFilePath, string.Empty),Times.Once());
        }

        [Test]
        public void It_Should_HasShown_Without_Searching_For_Marker_File_When_Showed_Is_Invoked()
        {
            shownReportWindowHistory.Showed();
            mocker.Verify<IFileUtil>(f => f.Exists(It.IsAny<string>()), Times.Never());
            Assert.That(shownReportWindowHistory.HasShown, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_Showed_Has_Not_Been_Invoked_Should_Search_For_Marker_File_Once_When_HasShown(bool fileExists)
        {
            mocker.GetMock<IFileUtil>().Setup(f => f.Exists(markerFilePath)).Returns(fileExists);

            void HasShownReportWindow()
            {
                var hasShown = shownReportWindowHistory.HasShown;
                Assert.That(hasShown, Is.EqualTo(fileExists));
            }
            HasShownReportWindow();
            HasShownReportWindow();

            mocker.Verify<IFileUtil>(f => f.Exists(markerFilePath), Times.Once());
        }
    }
}
