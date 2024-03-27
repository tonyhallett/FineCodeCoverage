using AutoMoq;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Core.Utilities;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverageTests.Initialization
{
    internal class FirstTimeReportWindowOpener_Tests
    {
        private AutoMoqer mocker;
        private FirstTimeReportWindowOpener firstTimeReportlWindowOpener;

        [SetUp]
        public void SetUp()  {
            mocker = new AutoMoqer();
            firstTimeReportlWindowOpener = mocker.Create<FirstTimeReportWindowOpener>();
        }

        [TestCase(true,false,true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public async Task It_Should_Open_If_Have_Never_Shown_The_ToolWindow_And_InitializedFromTestContainerDiscoverer_Async(
            bool initializedFromTestContainerDiscoverer,
            bool hasShownReportWindow,
            bool expectedShown
            )
        {
            mocker.GetMock<IInitializedFromTestContainerDiscoverer>().Setup(x => x.InitializedFromTestContainerDiscoverer).Returns(initializedFromTestContainerDiscoverer);
            mocker.GetMock<IShownReportWindowHistory>().Setup(x => x.HasShown).Returns(hasShownReportWindow);

            await firstTimeReportlWindowOpener.OpenIfFirstTimeAsync(CancellationToken.None);

            var expectedTimes = expectedShown ? Times.Once() : Times.Never();
            mocker.Verify<IReportWindowOpener>(reportWindowOpener => reportWindowOpener.OpenAsync(), expectedTimes);

        }
    }
}
