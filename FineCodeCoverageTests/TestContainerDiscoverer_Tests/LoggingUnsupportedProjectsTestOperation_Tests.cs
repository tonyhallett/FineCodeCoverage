using AutoMoq;
using FineCodeCoverage.Impl;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using System.Threading.Tasks;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class LoggingUnsupportedProjectsTestOperation_Tests
    {
        private AutoMoqer mocker;
        private LoggingUnsupportedProjectsTestOperation loggingUnsupportedProjectsTestOperation;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            loggingUnsupportedProjectsTestOperation = mocker.Create<LoggingUnsupportedProjectsTestOperation>();
        }

        [Test]
        public void Should_Return_FailedTests_From_Wrapped()
        {
            mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.FailedTests).Returns(42);

            Assert.AreEqual(42, loggingUnsupportedProjectsTestOperation.FailedTests);
        }

        [Test]
        public void Should_Return_TotalTests_From_Wrapped()
        {
            mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.TotalTests).Returns(42);

            Assert.AreEqual(42, loggingUnsupportedProjectsTestOperation.TotalTests);
        }

        [Test]
        public void Should_Return_SolutionDirectory_From_Wrapped()
        {
            mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.SolutionDirectory).Returns("SolnDirectory");

            Assert.AreEqual("SolnDirectory", loggingUnsupportedProjectsTestOperation.SolutionDirectory);
        }

        [Test]
        public async Task Should_Return_CoverageProjects_From_Wrapped()
        {
            var coverageProjects = new List<ICoverageProject>();
            mocker.GetMock<ITestOperation>().Setup(wrapped => wrapped.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);
            mocker.GetMock<ITestOperation>().Setup(wrapped => wrapped.UnsupportedProjects).Returns(new List<string> { });
            Assert.AreSame(coverageProjects, await loggingUnsupportedProjectsTestOperation.GetCoverageProjectsAsync());
        }

        [Test]
        public async Task Should_Log_Unsupported_Projects()
        {
            mocker.GetMock<ITestOperation>().SetupGet(wrapped => wrapped.UnsupportedProjects)
                .Returns(new List<string> { "Unsupported1", "Unsupported2"});

            await loggingUnsupportedProjectsTestOperation.GetCoverageProjectsAsync();

            IEnumerable<string> expectedLogged = new List<string> { "Unsupported projects: ", "Unsupported1", "Unsupported2" };
            mocker.Verify<ILogger>(logger => logger.Log(expectedLogged));
        }

    }
}