namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    using System;
    using System.Collections.Generic;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using Moq;
    using NUnit.Framework;

    internal class CoverageToolOutputFolderFromSolutionProvider_Tests
    {
        private List<int> callOrder;
        private AutoMoqer mocker;

        [SetUp]
        public void SetUp() => this.mocker = new AutoMoqer();

        private void SetUpProviders(bool provider1First, string provider1Provides, string provider2Provides)
        {
            this.callOrder = new List<int>();
            var mockOrderMetadata1 = new Mock<IOrderMetadata>();
            _ = mockOrderMetadata1.Setup(o => o.Order).Returns(provider1First ? 1 : 2);
            var mockOrderMetadata2 = new Mock<IOrderMetadata>();
            _ = mockOrderMetadata2.Setup(o => o.Order).Returns(provider1First ? 2 : 1);

            var mockCoverageToolOutputFolderProvider1 = new Mock<ICoverageToolOutputFolderSolutionProvider>();
            _ = mockCoverageToolOutputFolderProvider1.Setup(p => p.Provide(It.IsAny<Func<string>>())).Returns(provider1Provides).Callback(() => this.callOrder.Add(1));
            var mockCoverageToolOutputFolderProvider2 = new Mock<ICoverageToolOutputFolderSolutionProvider>();
            _ = mockCoverageToolOutputFolderProvider2.Setup(p => p.Provide(It.IsAny<Func<string>>())).Returns(provider2Provides).Callback(() => this.callOrder.Add(2));
            var lazyOrderedProviders = new List<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>>
            {
                new Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>(
                    ()=>mockCoverageToolOutputFolderProvider1.Object,mockOrderMetadata1.Object),
                new Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>(
                    ()=>mockCoverageToolOutputFolderProvider2.Object,mockOrderMetadata2.Object)
            };
            this.mocker.SetInstance<IEnumerable<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>>>(lazyOrderedProviders);
        }

        [Test]
        public void Should_Have_First_Order() => MefOrderAssertions.TypeHasExpectedOrder(
            typeof(CoverageToolOutputFolderFromSolutionProvider),
            1
        );

        [TestCase(true, 1, 2)]
        [TestCase(false, 2, 1)]
        public void Should_Use_Providers_In_Order(bool provider1First, int expectedFirst, int expectedSecond)
        {
            this.SetUpProviders(provider1First, null, null);
            var coverageToolOutputFolderFromSolutionProvider = this.mocker.Create<CoverageToolOutputFolderFromSolutionProvider>();
            _ = coverageToolOutputFolderFromSolutionProvider.Provide(null);
            Assert.That(new List<int> { expectedFirst, expectedSecond }, Is.EqualTo(this.callOrder));
        }
        //need to check if there is a coverage project ?
        [Test]
        public void Should_Stop_Asking_Providers_When_One_Returns_The_Folder()
        {
            this.SetUpProviders(true, "Folder", "_");
            var coverageToolOutputFolderFromSolutionProvider = this.mocker.Create<CoverageToolOutputFolderFromSolutionProvider>();
            Assert.Multiple(() =>
            {
                Assert.That(coverageToolOutputFolderFromSolutionProvider.Provide(null), Is.EqualTo("Folder"));
                Assert.That(new List<int> { 1 }, Is.EqualTo(this.callOrder));
            });
        }

        [Test]
        public void Should_Provide_The_Solution_Folder_Once_From_The_Solution_Folder_Provider_Wth_ProjectFile_Of_First_CoverageProject()
        {
            var mockProject1 = new Mock<ICoverageProject>();
            _ = mockProject1.Setup(p => p.ProjectFile).Returns("project.csproj");
            var mockProject2 = new Mock<ICoverageProject>();
            _ = mockProject2.Setup(p => p.ProjectFile).Returns("project2.csproj");
            var coverageProjects = new List<ICoverageProject> { mockProject1.Object, mockProject2.Object };

            var mockOrderMetadata1 = new Mock<IOrderMetadata>();
            _ = mockOrderMetadata1.Setup(o => o.Order).Returns(1);

            var mockCoverageToolOutputFolderProvider1 = new Mock<ICoverageToolOutputFolderSolutionProvider>();
            Func<string> solutionFolderProviderFunc = null;
            _ = mockCoverageToolOutputFolderProvider1.Setup(p => p.Provide(It.IsAny<Func<string>>()))
                .Callback<Func<string>>(solnFolderProvider => solutionFolderProviderFunc = solnFolderProvider);
            var lazyOrderedProviders = new List<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>>
            {
                new Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>(
                    ()=>mockCoverageToolOutputFolderProvider1.Object,mockOrderMetadata1.Object),
            };
            this.mocker.SetInstance<IEnumerable<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>>>(lazyOrderedProviders);

            var mockSolutionFolderProvider = this.mocker.GetMock<ISolutionFolderProvider>();
            _ = mockSolutionFolderProvider.Setup(sfp => sfp.Provide("project.csproj")).Returns("SolutionPath");
            var coverageToolOutputFolderFromSolutionProvider = this.mocker.Create<CoverageToolOutputFolderFromSolutionProvider>();

            _ = coverageToolOutputFolderFromSolutionProvider.Provide(coverageProjects);

            var solutionFolder = solutionFolderProviderFunc();
            var solutionFolder2 = solutionFolderProviderFunc();
            Assert.Multiple(() =>
            {
                Assert.That(solutionFolder, Is.EqualTo("SolutionPath"));
                Assert.That(solutionFolder2, Is.EqualTo("SolutionPath"));
            });
            mockSolutionFolderProvider.Verify(sfp => sfp.Provide("project.csproj"), Times.Once());
        }
    }
}
