namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System.Collections.Generic;
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using Moq;
    using NUnit.Framework;

    public class ShimCopier_Tests
    {
        private AutoMoqer autoMocker;
        private ShimCopier shimCopier;

        [SetUp]
        public void SetupSut()
        {
            this.autoMocker = new AutoMoqer();
            this.shimCopier = this.autoMocker.Create<ShimCopier>();
        }

        [Test]
        public void Should_Copy_Shim_For_Net_Framework_Projects_Where_Does_Not_Exist_In_Project_Output_Folder()
        {
            IEnumerable<ICoverageProject> coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject("NetFramework",true),
                this.CreateCoverageProject("",false),
            };

            var shimDestination = Path.Combine("NetFramework", Path.GetFileName("ShimPath"));
            var mockFileUtil = this.autoMocker.GetMock<IFileUtil>();

            _ = mockFileUtil.Setup(file => file.Exists(shimDestination)).Returns(false);
            _ = mockFileUtil.Setup(file => file.Copy("ShimPath", shimDestination));
            this.shimCopier.Copy("ShimPath", coverageProjects);

            mockFileUtil.VerifyAll();
            mockFileUtil.VerifyNoOtherCalls();
        }

        [Test]
        public void Should_Not_Copy_Shim_For_Net_Framework_Projects_If_Already_Exists()
        {
            IEnumerable<ICoverageProject> coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject("NetFramework",true),
                this.CreateCoverageProject("",false),
            };

            var shimDestination = Path.Combine("NetFramework", Path.GetFileName("ShimPath"));
            var mockFileUtil = this.autoMocker.GetMock<IFileUtil>();

            _ = mockFileUtil.Setup(file => file.Exists(shimDestination)).Returns(true);

            this.shimCopier.Copy("ShimPath", coverageProjects);

            mockFileUtil.Verify(file => file.Copy("ShimPath", shimDestination), Times.Never());
        }

        private ICoverageProject CreateCoverageProject(string projectOutputFolder, bool isNetFramework)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.SetupGet(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);
            _ = mockCoverageProject.SetupGet(cp => cp.IsDotNetFramework).Returns(isNetFramework);
            return mockCoverageProject.Object;
        }
    }
}
