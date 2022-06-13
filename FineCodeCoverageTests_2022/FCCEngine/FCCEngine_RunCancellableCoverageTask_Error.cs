namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System;
    using System.Threading.Tasks;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    internal class FCCEngine_RunCancellableCoverageTask_Error : FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        private Task RunCancellableCoverageTask_That_Throws_Async(Action cleanUp = null) =>
            this.RunCancellableCoverageTaskAsync((_) => throw new Exception("An exception occurred"), cleanUp);

        [Test]
        public async Task Should_Log_Async()
        {
            await this.RunCancellableCoverageTask_That_Throws_Async();

            this.Mocker.Verify<ILogger>(
                logger => logger.Log(
                    CoverageStatus.Error.Message(),
                    It.Is<Exception>(exc => exc.Message == "An exception occurred")
                )
            );

            this.Mocker.GetMock<IEventAggregator>().AssertLogToolWindowLinkShowFCCOutputPane(
                "An exception occurred. See the ", MessageContext.Error
            );
        }

        [Test]
        public async Task Should_Send_CoverageStoppedMessage_Async()
        {
            await this.RunCancellableCoverageTask_That_Throws_Async();

            this.Mocker.GetMock<IEventAggregator>().AssertCoverageStopped(Times.Once());
        }

        [Test]
        public async Task Should_Invoke_Clean_Up_If_Provided_Async()
        {
            var invokedCleanUp = false;
            await this.RunCancellableCoverageTask_That_Throws_Async(() => invokedCleanUp = true);

            Assert.That(invokedCleanUp, Is.True);
        }
    }

}
