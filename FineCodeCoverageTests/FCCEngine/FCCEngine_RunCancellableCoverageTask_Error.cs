using System;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Output.JsMessages.Logging;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.FCCEngine_Tests
{
    internal class FCCEngine_RunCancellableCoverageTask_Error : FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        private Task RunCancellableCoverageTask_That_Throws(Action cleanUp = null)
        {
            return RunCancellableCoverageTask((_) =>
            {
                throw new Exception("An exception occurred");
            }, cleanUp);

        }

        [Test]
        public async Task Should_Log()
        {
            await RunCancellableCoverageTask_That_Throws();

            mocker.Verify<ILogger>(
                logger => logger.Log(CoverageStatus.Error.Message(), It.Is<Exception>(exc => exc.Message == "An exception occurred"))
            );
            
            mocker.GetMock<IEventAggregator>().AssertLogToolWindowLinkShowFCCOutputPane("An exception occurred. See the ", MessageContext.Error);
        }

        [Test]
        public async Task Should_Send_CoverageStoppedMessage()
        {
            await RunCancellableCoverageTask_That_Throws();

            mocker.GetMock<IEventAggregator>().AssertCoverageStopped(Times.Once());
        }

        [Test]
        public async Task Should_Invoke_Clean_Up_If_Provided()
        {
            bool invokedCleanUp = false;
            await RunCancellableCoverageTask_That_Throws(() =>
            {
                invokedCleanUp = true;
            });

            Assert.True(invokedCleanUp);
        }
    }

}