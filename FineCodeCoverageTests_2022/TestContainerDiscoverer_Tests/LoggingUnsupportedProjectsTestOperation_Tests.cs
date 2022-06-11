namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Impl;
    using Moq;
    using NUnit.Framework;

    internal class LoggingUnsupportedProjectsTestOperation_Tests
    {
        private AutoMoqer mocker;
        private LoggingUnsupportedProjectsTestOperation loggingUnsupportedProjectsTestOperation;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.loggingUnsupportedProjectsTestOperation = this.mocker.Create<LoggingUnsupportedProjectsTestOperation>();
        }

        [Test]
        public void Should_Return_FailedTests_From_Wrapped()
        {
            _ = this.mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.FailedTests).Returns(42);

            Assert.That(this.loggingUnsupportedProjectsTestOperation.FailedTests, Is.EqualTo(42));
        }

        [Test]
        public void Should_Return_TotalTests_From_Wrapped()
        {
            _ = this.mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.TotalTests).Returns(42);

            Assert.That(this.loggingUnsupportedProjectsTestOperation.TotalTests, Is.EqualTo(42));
        }

        [Test]
        public void Should_Return_SolutionDirectory_From_Wrapped()
        {
            _ = this.mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.SolutionDirectory).Returns("SolnDirectory");

            Assert.That(this.loggingUnsupportedProjectsTestOperation.SolutionDirectory, Is.EqualTo("SolnDirectory"));
        }

        [Test]
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public async Task Should_Return_CoverageProjects_From_Wrapped()
        {
            var coverageProjects = new List<ICoverageProject>();
            _ = this.mocker.GetMock<ITestOperation>()
                .Setup(wrapped => wrapped.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);
            _ = this.mocker.GetMock<ITestOperation>()
                .Setup(wrapped => wrapped.UnsupportedProjects).Returns(new List<string> { });

            Assert.That(
                await this.loggingUnsupportedProjectsTestOperation.GetCoverageProjectsAsync(),
                Is.SameAs(coverageProjects)
            );
        }

        [Test]
        public async Task Should_Log_Unsupported_Projects()
        {
            _ = this.mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.UnsupportedProjects)
                .Returns(new List<string> { "Unsupported1", "Unsupported2" });

            _ = await this.loggingUnsupportedProjectsTestOperation.GetCoverageProjectsAsync();

            IEnumerable<string> expectedLogged = new List<string> { "Unsupported projects: ", "Unsupported1", "Unsupported2" };
            this.mocker.Verify<ILogger>(logger => logger.Log(expectedLogged));
        }
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
    }
}
