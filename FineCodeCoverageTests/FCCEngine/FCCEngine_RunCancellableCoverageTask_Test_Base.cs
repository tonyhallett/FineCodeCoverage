using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.FCCEngine_Tests
{
    internal abstract class FCCEngine_RunCancellableCoverageTask_Test_Base
    {
        protected AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            fccEngine = mocker.Create<FCCEngine>();
        }

        protected async Task RunCancellableCoverageTask(Func<CancellationToken, Task<List<CoverageLine>>> reportResultProvider, Action cleanUp = null)
        {
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(new CancellationTokenSource());
            mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider => taskProvider());

            var mockInitializeStatusProvider = new Mock<IInitializeStatusProvider>();
            mockInitializeStatusProvider.Setup(
                initializeStatusProvider => initializeStatusProvider.WaitForInitializedAsync(It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            fccEngine.Initialize(mockInitializeStatusProvider.Object, CancellationToken.None);

            fccEngine.RunCancellableCoverageTask(reportResultProvider, cleanUp);

            await fccEngine.reloadCoverageTask;
        }
    }

}