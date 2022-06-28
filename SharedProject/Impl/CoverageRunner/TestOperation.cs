using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Impl
{
    internal class TestOperation : ITestOperation
    {
        internal readonly TestRunRequest testRunRequest;
        internal readonly ICoverageProjectFactory coverageProjectFactory;
        internal readonly IRunSettingsRetriever runSettingsRetriever;
        internal readonly IOperation operation;

        public TestOperation(
            TestRunRequest testRunRequest, 
            ICoverageProjectFactory coverageProjectFactory, 
            IRunSettingsRetriever runSettingsRetriever,
            IOperation operation
        )
        {
            this.testRunRequest = testRunRequest;
            this.coverageProjectFactory = coverageProjectFactory;
            this.runSettingsRetriever = runSettingsRetriever;
            this.operation = operation;
        }
        public long FailedTests => testRunRequest.Response.FailedTests;

        public long TotalTests => testRunRequest.TotalTests;

        public string SolutionDirectory => testRunRequest.Configuration.SolutionDirectory;

        public List<string> UnsupportedProjects { get; private set; }

        public Task<List<ICoverageProject>> GetCoverageProjectsAsync()
        {
            return GetCoverageProjectsAsync(testRunRequest.Configuration);
        }

        public IEnumerable<Uri> GetRunSettingsDataCollectorResultUri(Uri collectorUri)
        {
            return operation.GetRunSettingsDataCollectorResultUri(collectorUri);
        }

        private async Task<List<ICoverageProject>> GetCoverageProjectsAsync(TestConfiguration testConfiguration)
        {
            var userRunSettings = testConfiguration.UserRunSettings;
            var testContainers = testConfiguration.Containers;
            List<ICoverageProject> coverageProjects = new List<ICoverageProject>();
            UnsupportedProjects = new List<string>();
            foreach (var container in testContainers)
            {
                var projectFilePath = container.ProjectData.ProjectFilePath;
                if (projectFilePath != null)
                {
                    var project = await coverageProjectFactory.CreateAsync();
                    coverageProjects.Add(project);

                    project.ProjectName = container.ProjectName;
                    project.TestDllFile = container.Source;
                    project.Is64Bit = container.TargetPlatform.ToString().ToLower().Equals("x64");
                    project.TargetFramework = container.TargetFramework.ToString();

                    var containerData = container.ProjectData;
                    project.Id = containerData.Id;
                    project.ProjectFile = projectFilePath;

                    project.RunSettingsFile = await runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData);
                }
                else
                {
                    UnsupportedProjects.Add(container.ProjectName);
                }

            }
            return coverageProjects;
        }

    }
}



