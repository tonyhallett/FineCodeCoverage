namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using Moq;
    using NUnit.Framework;

    internal class CoverageToolOutputManager_Tests
    {
        private AutoMoqer mocker;
        private Mock<ICoverageProject> mockProject1;
        private Mock<ICoverageProject> mockProject2;
        private List<ICoverageProject> coverageProjects;
        private List<int> callOrder;
        private const string DefaultCoverageFolder = "defaultFolder";

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mockProject1 = new Mock<ICoverageProject>();
            _ = this.mockProject1.Setup(p => p.FCCOutputFolder).Returns("p1output");
            _ = this.mockProject1.Setup(p => p.ProjectName).Returns("project1");
            _ = this.mockProject1.SetupProperty(p => p.CoverageOutputFolder);
            _ = this.mockProject1.Setup(p => p.DefaultCoverageOutputFolder).Returns(DefaultCoverageFolder);
            this.mockProject2 = new Mock<ICoverageProject>();
            _ = this.mockProject2.Setup(p => p.FCCOutputFolder).Returns("p2output");
            _ = this.mockProject2.Setup(p => p.ProjectName).Returns("project2");
            _ = this.mockProject2.Setup(p => p.DefaultCoverageOutputFolder).Returns(DefaultCoverageFolder);
            this.coverageProjects = new List<ICoverageProject> { this.mockProject1.Object, this.mockProject2.Object };
        }

        private void SetUpProviders(bool provider1First, string provider1Provides, string provider2Provides)
        {
            this.callOrder = new List<int>();
            var mockOrderMetadata1 = new Mock<IOrderMetadata>();
            _ = mockOrderMetadata1.Setup(o => o.Order).Returns(provider1First ? 1 : 2);
            var mockOrderMetadata2 = new Mock<IOrderMetadata>();
            _ = mockOrderMetadata2.Setup(o => o.Order).Returns(provider1First ? 2 : 1);

            var mockCoverageToolOutputFolderProvider1 = new Mock<ICoverageToolOutputFolderProvider>();
            _ = mockCoverageToolOutputFolderProvider1.Setup(p => p.Provide(this.coverageProjects)).Returns(provider1Provides).Callback(() => this.callOrder.Add(1));
            var mockCoverageToolOutputFolderProvider2 = new Mock<ICoverageToolOutputFolderProvider>();
            _ = mockCoverageToolOutputFolderProvider2.Setup(p => p.Provide(this.coverageProjects)).Returns(provider2Provides).Callback(() => this.callOrder.Add(2));
            var lazyOrderedProviders = new List<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>>
            {
                new Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>(
                    ()=>mockCoverageToolOutputFolderProvider1.Object,mockOrderMetadata1.Object),
                new Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>(
                    ()=>mockCoverageToolOutputFolderProvider2.Object,mockOrderMetadata2.Object)
            };
            this.mocker.SetInstance<IEnumerable<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>>>(lazyOrderedProviders);
        }

        [TestCase(true, 1, 2)]
        [TestCase(false, 2, 1)]
        public void Should_Use_Providers_In_Order_When_Determining_CoverageProject_Output_Folder(bool provider1First, int expectedFirst, int expectedSecond)
        {
            this.SetUpProviders(provider1First, null, null);
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);
            Assert.That(new List<int> { expectedFirst, expectedSecond }, Is.EqualTo(this.callOrder));
        }

        [Test]
        public void Should_Stop_Asking_Providers_When_One_Provides_Value()
        {
            this.SetUpProviders(true, "_", "_");
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);
            Assert.That(new List<int> { 1 }, Is.EqualTo(this.callOrder));
        }

        [Test]
        public void Should_Try_Empty_Provided_Output_Folder()
        {
            this.SetUpProviders(true, "Provided", "_");
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);
            this.mocker.Verify<IFileUtil>(f => f.TryEmptyDirectory("Provided"));
        }


        [Test]
        public void Should_Log_When_Provided()
        {
            this.SetUpProviders(true, "Provided", "_");
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);
            this.mocker.Verify<ILogger>(l => l.Log("FCC output in Provided"));
        }

        [Test]
        public void Should_Set_CoverageOutputFolder_To_ProjectName_Sub_Folder_Of_Provided()
        {
            this.SetUpProviders(true, "Provided", "_");
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);

            var expectedProject1OutputFolder = Path.Combine("Provided", this.mockProject1.Object.ProjectName);
            var expectedProject2OutputFolder = Path.Combine("Provided", this.mockProject2.Object.ProjectName);
            this.mockProject1.VerifySet(p => p.CoverageOutputFolder = expectedProject1OutputFolder);
            this.mockProject2.VerifySet(p => p.CoverageOutputFolder = expectedProject2OutputFolder);

        }

        [Test]
        public void Should_Set_CoverageOutputFolder_To_Default_For_All_When_Not_Provided()
        {
            this.SetUpProviders(true, null, null);
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);


            this.mockProject1.VerifySet(p => p.CoverageOutputFolder = DefaultCoverageFolder);
            this.mockProject2.VerifySet(p => p.CoverageOutputFolder = DefaultCoverageFolder);
        }

        [Test]
        public void Should_Output_Reports_To_First_Project_CoverageOutputFolder_When_Not_Provided()
        {
            this.SetUpProviders(true, null, null);
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);

            var firstProjectOutputFolder = this.mockProject1.Object.CoverageOutputFolder;

            Assert.That(firstProjectOutputFolder, Is.EqualTo(coverageToolOutputManager.GetReportOutputFolder()));
        }

        [Test]
        public void Should_Output_Reports_To_Provided_When_Provided()
        {
            this.SetUpProviders(true, "Provided", null);
            var coverageToolOutputManager = this.mocker.Create<CoverageToolOutputManager>();
            coverageToolOutputManager.SetProjectCoverageOutputFolder(this.coverageProjects);

            var outputFolder = coverageToolOutputManager.GetReportOutputFolder();

            Assert.That(outputFolder, Is.EqualTo("Provided"));

        }

    }
}
