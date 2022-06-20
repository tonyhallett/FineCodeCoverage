namespace FineCodeCoverageTests.Coverlet_Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Engine.Coverlet;
    using FineCodeCoverage.Engine.Model;
    using Moq;
    using NUnit.Framework;

    public class CoverletUtil_Tests
    {
        private AutoMoqer mocker;
        private CoverletUtil coverletUtil;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.coverletUtil = this.mocker.Create<CoverletUtil>();
        }

        [Test]
        public async Task Should_Use_The_DataCollector_If_Possible_Async()
        {
            var ct = CancellationToken.None;
            var project = new Mock<ICoverageProject>().Object;

            var mockDataCollectorUtil = this.mocker.GetMock<ICoverletDataCollectorUtil>();
            _ = mockDataCollectorUtil.Setup(dc => dc.CanUseDataCollector(project)).Returns(true);
            _ = mockDataCollectorUtil.Setup(dc => dc.RunAsync(ct));

            await this.coverletUtil.RunCoverletAsync(project, ct);

            mockDataCollectorUtil.VerifyAll();
        }

        [Test]
        public async Task Should_Use_The_Global_Tool_If_Not_Possible_Async()
        {
            var ct = CancellationToken.None;
            var project = new Mock<ICoverageProject>().Object;

            var mockDataCollectorUtil = this.mocker.GetMock<ICoverletDataCollectorUtil>();
            _ = mockDataCollectorUtil.Setup(dc => dc.CanUseDataCollector(project)).Returns(false);

            var mockGlobalUtil = this.mocker.GetMock<ICoverletConsoleUtil>();
            _ = mockGlobalUtil.Setup(g => g.RunAsync(project, ct));

            await this.coverletUtil.RunCoverletAsync(project, ct);

            mockDataCollectorUtil.VerifyAll();
            mockGlobalUtil.VerifyAll();
        }
    }
}
