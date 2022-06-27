namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using Moq;
    using NUnit.Framework;

    internal class FCCEngine_UI_RunCancellableCoverageTask_Success : FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        [Test]
        public async Task Should_Send_NewCoverageLinesMessage_Async()
        {
            var coverageLines = new List<CoverageLine>();
            await this.RunCancellableCoverageTaskAsync((_) => Task.FromResult(coverageLines));


            this.Mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.SendMessage(
                    It.Is<NewCoverageLinesMessage>(
                        newCoverageLinesMessage => newCoverageLinesMessage.CoverageLines == coverageLines
                    ),
                    null
                )
            );
        }
    }
}
