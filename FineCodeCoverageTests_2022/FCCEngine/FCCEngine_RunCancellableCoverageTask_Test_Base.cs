namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using Moq;
    using NUnit.Framework;

    internal abstract class FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        protected AutoMoqer Mocker { get; private set; }

        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            this.Mocker = new AutoMoqer();
            this.fccEngine = this.Mocker.Create<FCCEngine>();
        }

        protected async Task RunCancellableCoverageTaskAsync(Func<CancellationToken, Task<List<CoverageLine>>> reportResultProvider, Action cleanUp = null)
        {
            var mockDisposeAwareTaskRunner = this.Mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(new CancellationTokenSource());
            _ = mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider => taskProvider());

            this.fccEngine.RunCancellableCoverageTask(reportResultProvider, cleanUp);

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await this.fccEngine.reloadCoverageTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }
    }

}
