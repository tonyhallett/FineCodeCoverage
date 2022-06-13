namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using NUnit.Framework;

    internal class FCCEngine_RunCancellableCoverageTask_Success : FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        private Task Run_Successful_Cancellable_Coverage_Task_Async(Action cleanUp = null) =>
            this.RunCancellableCoverageTaskAsync((_) => Task.FromResult(new List<CoverageLine>()), cleanUp);

        [Test]
        public async Task Should_Log_Async()

        {
            await this.Run_Successful_Cancellable_Coverage_Task_Async();

            this.Mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Done.Message()));
            this.Mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("Coverage completed", MessageContext.CoverageCompleted);
        }

        [Test]
        public async Task Should_Invoke_Clean_Up_If_Provided_Async()
        {
            var invokedCleanUp = false;
            await this.Run_Successful_Cancellable_Coverage_Task_Async(() => invokedCleanUp = true);

            Assert.That(invokedCleanUp, Is.True);
        }
    }

}
