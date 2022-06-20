namespace FineCodeCoverageTests.OldStyleCoverage_Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Coverlet;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.OpenCover;
    using Moq;
    using NUnit.Framework;

    internal class CoverageUtilManager_Tests
    {
        private AutoMoqer mocker;
        [SetUp]
        public void SetUp() => this.mocker = new AutoMoqer();

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Run_The_Appropriate_Cover_Tool_Based_On_IsDotNetSdkStyle_Async(bool isDotNetSdkStyle)
        {
            var mockProject = new Mock<ICoverageProject>();
            _ = mockProject.Setup(cp => cp.IsDotNetSdkStyle()).Returns(isDotNetSdkStyle);
            var mockedProject = mockProject.Object;

            var coverageUtilManager = this.mocker.Create<CoverageUtilManager>();
            var ct = CancellationToken.None;
            await coverageUtilManager.RunCoverageAsync(mockedProject, ct);

            if (isDotNetSdkStyle)
            {
                this.mocker.Verify<ICoverletUtil>(coverletUtil => coverletUtil.RunCoverletAsync(mockedProject, ct));
            }
            else
            {
                this.mocker.Verify<IOpenCoverUtil>(openCoverUtil => openCoverUtil.RunOpenCoverAsync(mockedProject, ct));
            }
        }
    }
}
