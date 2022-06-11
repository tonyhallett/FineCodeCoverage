using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output.JsMessages.Logging;
using NUnit.Framework;

namespace FineCodeCoverageTests.FCCEngine_Tests
{
    internal class FCCEngine_RunCancellableCoverageTask_Success : FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        private Task Run_Successful_Cancellable_Coverage_Task_Async(Action cleanUp = null)
        {
            return this.RunCancellableCoverageTaskAsync((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, cleanUp);
        }

        [Test]
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public async Task Should_Log()

        {
            await this.Run_Successful_Cancellable_Coverage_Task_Async();

            this.Mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Done.Message()));
            this.Mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("Coverage completed", MessageContext.CoverageCompleted);
        }

        [Test]
        public async Task Should_Invoke_Clean_Up_If_Provided()
        {
            bool invokedCleanUp = false;
            await this.Run_Successful_Cancellable_Coverage_Task_Async(() =>
            {
                invokedCleanUp = true;
            });

            Assert.That(invokedCleanUp, Is.True);
        }
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
    }

}