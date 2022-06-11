using AutoMoq;
using FineCodeCoverage.Impl;
using Moq;
using NUnit.Framework;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestOperationFactory_Tests
    {
        [Test]
        public void Should_Create_LoggingUnsupportedProjectsTestOperation_Wrapping_TestOperation()
        {
            var mocker = new AutoMoqer();
            var testOperationFactory = mocker.Create<TestOperationFactory>();
            var operation = new Mock<IOperation>().Object;

            var testRunRequest = new TestRunRequest();
            var mockTestRunRequestFactory = mocker.GetMock<ITestRunRequestFactory>();
            mockTestRunRequestFactory.Setup(testRunRequestFactory => testRunRequestFactory.Create(operation)).Returns(testRunRequest);

            var loggingUnsupportedProjectsTestOperation = testOperationFactory.Create(operation) as LoggingUnsupportedProjectsTestOperation;
            var testOperation = loggingUnsupportedProjectsTestOperation.wrappedTestOperation as TestOperation;

            Assert.AreSame(testRunRequest, testOperation.testRunRequest);
            Assert.AreSame(mocker.GetMock<IRunSettingsRetriever>().Object, testOperation.runSettingsRetriever);
            Assert.AreSame(mocker.GetMock<ICoverageProjectFactory>().Object, testOperation.coverageProjectFactory);
        }
    }
}