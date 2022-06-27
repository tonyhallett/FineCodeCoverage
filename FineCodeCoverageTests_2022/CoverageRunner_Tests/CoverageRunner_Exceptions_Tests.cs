namespace FineCodeCoverageTests.CoverageRunner_Tests
{
    using System;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    internal class CoverageRunner_Exceptions_Tests : CoverageRunner_Tests_Base
    {
        private readonly Exception exception = new Exception("an exception");

        [SetUp]
        public void SetupForException()
        {
            var mockCoverageService = new Mock<ICoverageService>();
            _ = mockCoverageService.Setup(coverageService => coverageService.StopCoverage()).Throws(this.exception);
            this.CoverageRunner.coverageService = mockCoverageService.Object;

            this.RaiseTestExecutionStarting();
        }

        [Test]
        public void Should_Handle_And_Log_Exceptions() => this.Mocker.Verify<ILogger>(
            logger => logger.Log("Error processing unit test events", this.exception)
        );

        [Test]
        public void Should_Tool_Window_Log_See_FCC_Output_Pane() =>
            this.Mocker.GetMock<IEventAggregator>().AssertLogToolWindowLinkShowFCCOutputPane(
                "Error processing unit test events, see ",
                MessageContext.Error
            );
    }
}
