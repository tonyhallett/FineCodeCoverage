namespace FineCodeCoverageTests.CoverageRunner_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Impl;
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using Moq;
    using NUnit.Framework;

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
            _ = mockTestRunRequestFactory.Setup(testRunRequestFactory => testRunRequestFactory.Create(operation)).Returns(testRunRequest);

            var loggingUnsupportedProjectsTestOperation = testOperationFactory.Create(operation) as LoggingUnsupportedProjectsTestOperation;
            var testOperation = loggingUnsupportedProjectsTestOperation.wrappedTestOperation as TestOperation;

            Assert.Multiple(() =>
            {
                Assert.That(testOperation.testRunRequest, Is.SameAs(testRunRequest));
                Assert.That(testOperation.runSettingsRetriever, Is.SameAs(mocker.GetMock<IRunSettingsRetriever>().Object));
                Assert.That(testOperation.coverageProjectFactory, Is.SameAs(mocker.GetMock<ICoverageProjectFactory>().Object));
            });
        }
    }
}
