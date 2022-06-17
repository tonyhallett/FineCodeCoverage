namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

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

            public bool IsDotNetSdkStyle() => throw new NotImplementedException();

            public Task<CoverageProjectFileSynchronizationDetails> PrepareForCoverageAsync(
                CancellationToken cancellationToken, bool synchronizeBuildOuput = true
            ) => throw new NotImplementedException();
            #endregion
        }

        private class UserRunSettings { }

        private class TargetObjectProperty
        {
            private readonly string toString;

            public TargetObjectProperty(string toString) => this.toString = toString;
            public override string ToString() => this.toString;

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

            Assert.That(testOperation.FailedTests, Is.EqualTo(42));
        }

        [Test]
        public void Should_Return_TotalTests_From_The_Request()
        {
            var testRunRequest = new TestRunRequest
            {
                TotalTests = 42
            };
            var testOperation = new TestOperation(testRunRequest, null, null);

            Assert.That(testOperation.TotalTests, Is.EqualTo(42));
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

            Assert.That(testOperation.SolutionDirectory, Is.EqualTo("SolnDirectory"));
        }

        [Test]
        public async Task Should_Set_CoverageProject_Properties_From_The_TestConfiguration_Async()
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
            _ = mockRunSettingsRetriever.Setup(
                runSettingsRetriever => runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData1)
            ).ReturnsAsync("runsettings1");
            _ = mockRunSettingsRetriever.Setup(
                runSettingsRetriever => runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData2)
            ).ReturnsAsync("runsettings2");

            var mockCoverageProjectFactory = new Mock<ICoverageProjectFactory>();
            _ = mockCoverageProjectFactory.
                Setup(coverageProjectFactory => coverageProjectFactory.CreateAsync()).
                ReturnsAsync(() => new TestCoverageProject());

            var testOperation = new TestOperation(testRunRequest, mockCoverageProjectFactory.Object, mockRunSettingsRetriever.Object);

            var coverageProjects = await testOperation.GetCoverageProjectsAsync();
            Assert.That(coverageProjects, Has.Count.EqualTo(2));

            Assert.Multiple(() =>
            {
                var firstCoverageProject = coverageProjects[0];
                var secondCoverageProject = coverageProjects[1];

                Assert.That(firstCoverageProject.ProjectName, Is.EqualTo("Project1"));
                Assert.That(secondCoverageProject.ProjectName, Is.EqualTo("Project2"));
                Assert.That(firstCoverageProject.TestDllFile, Is.EqualTo("dll1"));
                Assert.That(secondCoverageProject.TestDllFile, Is.EqualTo("dll2"));
                Assert.That(firstCoverageProject.Is64Bit, Is.False);
                Assert.That(secondCoverageProject.Is64Bit, Is.True);
                Assert.That(firstCoverageProject.TargetFramework, Is.EqualTo("TargetFramework1"));
                Assert.That(secondCoverageProject.TargetFramework, Is.EqualTo("TargetFramework2"));
                Assert.That(firstCoverageProject.ProjectFile, Is.EqualTo("projectFilePath1"));
                Assert.That(secondCoverageProject.ProjectFile, Is.EqualTo("projectFilePath2"));
                Assert.That(firstCoverageProject.RunSettingsFile, Is.EqualTo("runsettings1"));
                Assert.That(secondCoverageProject.RunSettingsFile, Is.EqualTo("runsettings2"));
                Assert.That(firstCoverageProject.Id, Is.EqualTo(containerData1.Id));
                Assert.That(secondCoverageProject.Id, Is.EqualTo(containerData2.Id));

                Assert.That(testOperation.UnsupportedProjects, Is.Empty);
            });
        }

        [Test]
        public async Task Should_Set_UnsupportedProjects_When_GetCoverageProjects_Async()
        {
            var testConfiguration = new TestConfiguration { };
            var containerData = new ContainerData
            {
                Id = Guid.NewGuid()
            };

            var containers = new List<Container>
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

            _ = await testOperation.GetCoverageProjectsAsync();

            Assert.That(testOperation.UnsupportedProjects, Is.EqualTo(new List<string> { "Unsupported1", "Unsupported2" }));

        }
    }
}
