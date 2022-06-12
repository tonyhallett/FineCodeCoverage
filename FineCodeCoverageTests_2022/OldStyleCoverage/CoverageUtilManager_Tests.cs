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

        [Test]
        public void Initialize_Should_Initialize_The_Coverage_Utils()
        {
            var coverageUtilManager = this.mocker.Create<CoverageUtilManager>();

            var ct = CancellationToken.None;
            coverageUtilManager.Initialize("AppDataFolder", ct);

            this.mocker.Verify<ICoverletUtil>(coverletUtil => coverletUtil.Initialize("AppDataFolder", ct));
            this.mocker.Verify<IOpenCoverUtil>(coverletUtil => coverletUtil.Initialize("AppDataFolder", ct));
        }

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
