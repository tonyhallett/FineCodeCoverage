using FineCodeCoverage.Core;
using FineCodeCoverage.Core.Utilities;
using Moq;
using NUnit.Framework;
using System;
using FineCodeCoverage.Output.JsMessages.Logging;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestContainerDiscoverer_Exceptions_Tests : TestContainerDiscoverer_Tests_Base
    {
        private readonly Exception exception = new Exception("an exception");

        protected override void AdditionalSetup()
        {
            var mockCoverageService = new Mock<ICoverageService>();
            mockCoverageService.Setup(coverageService => coverageService.StopCoverage()).Throws(exception);
            testContainerDiscoverer.coverageService = mockCoverageService.Object;

            RaiseTestExecutionStarting();
        }

        [Test]
        public void Should_Handle_And_Log_Exceptions()
        {
            mocker.Verify<ILogger>(logger => logger.Log("Error processing unit test events", exception));
        }

        [Test]
        public void Should_Tool_Window_Log_See_FCC_Output_Pane()
        {
            mocker.GetMock<IEventAggregator>().AssertLogToolWindowLinkShowFCCOutputPane(
                "Error processing unit test events, see ", 
                MessageContext.Error
            );
        }
    }
}