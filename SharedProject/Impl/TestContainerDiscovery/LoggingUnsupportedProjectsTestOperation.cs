using FineCodeCoverage.Engine.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FineCodeCoverage.Impl
{
    internal class LoggingUnsupportedProjectsTestOperation : ITestOperation
    {
        internal readonly ITestOperation wrappedTestOperation;
        private readonly ILogger logger;

        public LoggingUnsupportedProjectsTestOperation(
           ITestOperation wrappedTestOperation,
           ILogger logger
        )
        {
            this.wrappedTestOperation = wrappedTestOperation;
            this.logger = logger;
        }

        public long FailedTests => wrappedTestOperation.FailedTests;

        public long TotalTests => wrappedTestOperation.TotalTests;

        public List<string> UnsupportedProjects => wrappedTestOperation.UnsupportedProjects;

        public string SolutionDirectory => wrappedTestOperation.SolutionDirectory;

        public async Task<List<ICoverageProject>> GetCoverageProjectsAsync()
        {
            var coverageProjects = await wrappedTestOperation.GetCoverageProjectsAsync();
            if (wrappedTestOperation.UnsupportedProjects.Count > 0)
            {
                logger.Log(new string[] { "Unsupported projects: " }.Concat(wrappedTestOperation.UnsupportedProjects));
            }
            return coverageProjects;
        }
    }
}
