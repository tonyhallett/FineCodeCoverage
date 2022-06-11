using FineCodeCoverage.Impl;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Options;
using System.Threading;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestOperation_Tests
    {
        private class TestCoverageProject : ICoverageProject
        {
            public string ProjectName { get; set; }
            public string TestDllFile { get; set; }
            public bool Is64Bit { get; set; }
            public string TargetFramework { get; set; }
            public Guid Id { get; set; }
            public string ProjectFile { get; set; }
            public string RunSettingsFile { get; set; }

            #region not implemented
            public string FCCOutputFolder => throw new NotImplementedException();

            public string CoverageOutputFile => throw new NotImplementedException();

            public string CoverageOutputFolder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string DefaultCoverageOutputFolder => throw new NotImplementedException();

            public List<string> ExcludedReferencedProjects => throw new NotImplementedException();

            public List<string> IncludedReferencedProjects => throw new NotImplementedException();

            public XElement ProjectFileXElement => throw new NotImplementedException();
            
            public string ProjectOutputFolder => throw new NotImplementedException();

            public IAppOptions Settings => throw new NotImplementedException();

            public bool IsDotNetFramework => throw new NotImplementedException();

            public bool IsDotNetSdkStyle()
            {
                throw new NotImplementedException();
            }

            public Task<CoverageProjectFileSynchronizationDetails> PrepareForCoverageAsync(CancellationToken cancellationToken, bool synchronizeBuildOuput = true)
            {
                throw new NotImplementedException();
            }
            #endregion
        }

        private class UserRunSettings { }
        
        private class TargetObjectProperty {
            private readonly string toString;

            public TargetObjectProperty(string toString)
            {
                this.toString = toString;
            }
            public override string ToString()
            {
                return toString;
            }

        }


        [Test]
        public void Should_Return_FailedTests_From_The_Response()
        {
            var testRunRequest = new TestRunRequest
            {
                Response = new TestRunResponse
                {
                    FailedTests = 42
                }
            };
            var testOperation = new TestOperation(testRunRequest, null, null);

            Assert.AreEqual(42, testOperation.FailedTests);
        }

        [Test]
        public void Should_Return_TotalTests_From_The_Request()
        {
            var testRunRequest = new TestRunRequest
            {
                TotalTests = 42
            };
            var testOperation = new TestOperation(testRunRequest, null, null);

            Assert.AreEqual(42, testOperation.TotalTests);
        }

        [Test]
        public void Should_Return_SolutionDirectory_From_The_Configuration()
        {
            var testRunRequest = new TestRunRequest
            {
                Configuration = new TestConfiguration
                {
                    SolutionDirectory = "SolnDirectory"
                }
            };
            var testOperation = new TestOperation(testRunRequest, null, null);

            Assert.AreEqual("SolnDirectory", testOperation.SolutionDirectory);
        }

        [Test]
        public async Task Should_Set_CoverageProject_Properties_From_The_TestConfiguration()
        {
            var userRunSettings = new UserRunSettings();
            var containerData1 = new ContainerData
            {
                ProjectFilePath = "projectFilePath1",
                Id = Guid.NewGuid()
            };

            var containerData2 = new ContainerData
            {
                ProjectFilePath = "projectFilePath2",
                Id = Guid.NewGuid()
            };

            var testConfiguration = new TestConfiguration
            {
                UserRunSettings = userRunSettings,
                Containers = new List<Container>
                {
                    new Container
                    {
                        ProjectData = containerData1,
                        ProjectName = "Project1",
                        TargetFramework = new TargetObjectProperty("TargetFramework1"),
                        TargetPlatform = new TargetObjectProperty("Not x64"),
                        Source = "dll1"
                    },
                    new Container
                    {
                        ProjectData = containerData2,
                        ProjectName = "Project2",
                        TargetFramework = new TargetObjectProperty("TargetFramework2"),
                        TargetPlatform = new TargetObjectProperty("x64"),
                        Source = "dll2"
                    }
                }
            };

            var testRunRequest = new TestRunRequest
            {
                Configuration = testConfiguration
            };

            var mockRunSettingsRetriever = new Mock<IRunSettingsRetriever>();
            mockRunSettingsRetriever.Setup(
                runSettingsRetriever => runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData1)
            ).ReturnsAsync("runsettings1");
            mockRunSettingsRetriever.Setup(
                runSettingsRetriever => runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData2)
            ).ReturnsAsync("runsettings2");

            var mockCoverageProjectFactory = new Mock<ICoverageProjectFactory>();
            mockCoverageProjectFactory.
                Setup(coverageProjectFactory => coverageProjectFactory.Create()).
                Returns(() => new TestCoverageProject());

            var testOperation = new TestOperation(testRunRequest, mockCoverageProjectFactory.Object, mockRunSettingsRetriever.Object);

            var coverageProjects = await testOperation.GetCoverageProjectsAsync();
            Assert.AreEqual(2, coverageProjects.Count);
            var firstCoverageProject = coverageProjects[0];
            var secondCoverageProject = coverageProjects[1];

            Assert.AreEqual("Project1", firstCoverageProject.ProjectName);
            Assert.AreEqual("Project2", secondCoverageProject.ProjectName);
            Assert.AreEqual("dll1", firstCoverageProject.TestDllFile);
            Assert.AreEqual("dll2", secondCoverageProject.TestDllFile);
            Assert.False(firstCoverageProject.Is64Bit);
            Assert.True(secondCoverageProject.Is64Bit);
            Assert.AreEqual("TargetFramework1", firstCoverageProject.TargetFramework);
            Assert.AreEqual("TargetFramework2", secondCoverageProject.TargetFramework);
            Assert.AreEqual("projectFilePath1", firstCoverageProject.ProjectFile);
            Assert.AreEqual("projectFilePath2", secondCoverageProject.ProjectFile);
            Assert.AreEqual("runsettings1", firstCoverageProject.RunSettingsFile);
            Assert.AreEqual("runsettings2", secondCoverageProject.RunSettingsFile);
            Assert.AreEqual(containerData1.Id, firstCoverageProject.Id);
            Assert.AreEqual(containerData2.Id, secondCoverageProject.Id);

            Assert.IsEmpty(testOperation.UnsupportedProjects);
        }

        [Test]
        public async Task Should_Set_UnsupportedProjects_When_GetCoverageProjectsAsync()
        {
            var userRunSettings = new UserRunSettings();
            var testConfiguration = new TestConfiguration { };
            var containerData = new ContainerData
            {
                Id = Guid.NewGuid()
            };

            List<Container> containers = new List<Container>
            {
                new Container
                {
                    ProjectData = containerData,
                    ProjectName = "Unsupported1"
                },
                new Container
                {
                    ProjectData = containerData,
                    ProjectName = "Unsupported2"
                },
            };
            testConfiguration.Containers = containers;
            var testRunRequest = new TestRunRequest
            {
                Configuration = testConfiguration
            };

            var testOperation = new TestOperation(testRunRequest, null, null);

            await testOperation.GetCoverageProjectsAsync();

            Assert.AreEqual(new List<string> { "Unsupported1", "Unsupported2" }, testOperation.UnsupportedProjects);

        }

    }
}