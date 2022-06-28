using System.ComponentModel.Composition;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ILogger = FineCodeCoverage.Logging.ILogger;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(ITestOperationFactory))]
    internal class TestOperationFactory : ITestOperationFactory
    {
        private readonly ICoverageProjectFactory coverageProjectFactory;
        private readonly IRunSettingsRetriever runSettingsRetriever;
        private readonly ITestRunRequestFactory testRunRequestFactory;
        private readonly ILogger logger;

        [ImportingConstructor]
        public TestOperationFactory(
            ICoverageProjectFactory coverageProjectFactory,
            IRunSettingsRetriever runSettingsRetriever,
            ITestRunRequestFactory testRunRequestFactory,
            ILogger logger
            )
        {
            this.coverageProjectFactory = coverageProjectFactory;
            this.runSettingsRetriever = runSettingsRetriever;
            this.testRunRequestFactory = testRunRequestFactory;
            this.logger = logger;
        }
        public ITestOperation Create(IOperation operation)
        {
            return new LoggingUnsupportedProjectsTestOperation(
                new TestOperation(
                    testRunRequestFactory.Create(operation),
                    coverageProjectFactory,
                    runSettingsRetriever,
                    operation
                ),
                logger
            );
        }
    }
}



