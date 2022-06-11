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
        private Task Run_Successful_Cancellable_Coverage_Task(Action cleanUp = null)
        {
            return RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, cleanUp);
        }

        [Test]
        public async Task Should_Log()
        {
            await Run_Successful_Cancellable_Coverage_Task();

            mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Done.Message()));
            mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("Coverage completed", MessageContext.CoverageCompleted);
        }

        [Test]
        public async Task Should_Invoke_Clean_Up_If_Provided()
        {
            bool invokedCleanUp = false;
            await Run_Successful_Cancellable_Coverage_Task(() =>
            {
                invokedCleanUp = true;
            });

            Assert.True(invokedCleanUp);
        }
    }

}