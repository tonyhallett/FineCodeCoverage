using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Output.JsMessages.Logging;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.FCCEngine_Tests
{
    public class FCCEngine_CancellationToken_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            
            fccEngine = mocker.Create<FCCEngine>();
        }

        [Test]
        public void Should_Cancel_The_Vs_Linked_CancellationTokenSource_When_StopCoverage()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            fccEngine.cancellationTokenSource = cancellationTokenSource;
            fccEngine.StopCoverage();
            Assert.True(cancellationTokenSource.IsCancellationRequested);
        }

        [Test]
        public void Should_Not_Throw_If_No_CancellationTokenSource_When_StopCoverage()
        {
            fccEngine.StopCoverage();
        }

        [Test]
        public void Should_Not_Throw_If_CancellationTokenSource_Is_Disposed_When_StopCoverage()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            fccEngine.cancellationTokenSource = cancellationTokenSource;
            cancellationTokenSource.Dispose();
            fccEngine.StopCoverage();
        }

        [Test]
        public void Should_Cancel_The_Vs_Linked_CancellationTokenSource_When_RunCancellableCoverageTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            fccEngine.cancellationTokenSource = cancellationTokenSource;

            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(new CancellationTokenSource());

            fccEngine.RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            },null);
            Assert.True(cancellationTokenSource.IsCancellationRequested);
        }

        [Test]
        public void Should_Set_A_New_Vs_Linked_CancellationTokenSource_When_RunCancellableCoverageTask()
        {
            var vsLinkedCancellationTokenSource = new CancellationTokenSource();
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(vsLinkedCancellationTokenSource);

            fccEngine.RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, null);

            Assert.AreSame(vsLinkedCancellationTokenSource, fccEngine.cancellationTokenSource);
            Assert.False(vsLinkedCancellationTokenSource.IsCancellationRequested);
        }
    
        [Test]
        public void Should_Run_A_Dispose_Aware_Task_When_RunCancellableCoverageTask()
        {
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(new CancellationTokenSource());

            fccEngine.RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, null);

            mockDisposeAwareTaskRunner.Verify(disposeAwareTaskRunner => disposeAwareTaskRunner.RunAsync(It.IsAny<Func<Task>>()));
        }

        [Test]
        public async Task Should_Pass_The_Vs_Shutdown_Linked_CancellationToken_To_The_ReportResultProvider_When_RunCancellableCoverageTask()
        {
            var linkedCancellationTokenSource = new CancellationTokenSource();
            var linkedCancellationToken = linkedCancellationTokenSource.Token;

            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(linkedCancellationTokenSource);
            mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider => taskProvider());

            var mockInitializeStatusProvider = new Mock<IInitializeStatusProvider>();
            mockInitializeStatusProvider.Setup(
                initializeStatusProvider => initializeStatusProvider.WaitForInitializedAsync(It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            fccEngine.Initialize(mockInitializeStatusProvider.Object, CancellationToken.None);


            CancellationToken ct = CancellationToken.None;
            fccEngine.RunCancellableCoverageTask((_ct) =>
            {
                ct = _ct;
                return Task.FromResult(new List<CoverageLine>());
            }, null);

            await fccEngine.reloadCoverageTask;

            Assert.AreEqual(linkedCancellationToken, ct);
        }

        [Test]
        public async Task Should_Poll_The_InitializedStatus_With_the_Vs_Linked_CancellationToken_When_RunCancellableCoverageTask()
        {
            var vsLinkedCancellationTokenSource = new CancellationTokenSource();
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(vsLinkedCancellationTokenSource);

            mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider => taskProvider());

            var mockInitializeStatusProvider = new Mock<IInitializeStatusProvider>();
            mockInitializeStatusProvider.Setup(
                initializeStatusProvider => initializeStatusProvider.WaitForInitializedAsync(vsLinkedCancellationTokenSource.Token)
            ).Returns(Task.CompletedTask);

            fccEngine.Initialize(mockInitializeStatusProvider.Object, CancellationToken.None);

            fccEngine.RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, null);

            await fccEngine.reloadCoverageTask;
            mockInitializeStatusProvider.VerifyAll();
        }

        [Test]
        public async Task Should_Dispose_CancellationTokenSource_For_That_Coverage_Task_When_Completes()
        {
            var laterRunCancellationTokenSource = new CancellationTokenSource();
            var currentRunCancellationTokenSource = new CancellationTokenSource();

            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(currentRunCancellationTokenSource);
            mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider =>
            {
                fccEngine.cancellationTokenSource = laterRunCancellationTokenSource;
                taskProvider();
            });

            var mockInitializeStatusProvider = new Mock<IInitializeStatusProvider>();
            mockInitializeStatusProvider.Setup(
                initializeStatusProvider => initializeStatusProvider.WaitForInitializedAsync(It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            fccEngine.Initialize(mockInitializeStatusProvider.Object, CancellationToken.None);


            fccEngine.RunCancellableCoverageTask((_ct) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, null);

            await fccEngine.reloadCoverageTask;

            Assert.True(currentRunCancellationTokenSource.IsDisposed());
            Assert.False(laterRunCancellationTokenSource.IsDisposed());
        }

        private async Task Run_Cancelled_Coverage_Task(Action cleanUp = null)
        {
            var cancelledCancellationTokenSource = new CancellationTokenSource();
            cancelledCancellationTokenSource.Cancel();

            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(cancelledCancellationTokenSource);
            mockDisposeAwareTaskRunner.Setup(runner => runner.RunAsync(It.IsAny<Func<Task>>())).Callback<Func<Task>>(taskProvider => taskProvider());

            fccEngine.RunCancellableCoverageTask((_) =>
            {
                return Task.FromResult(new List<CoverageLine>());
            }, cleanUp);
            await fccEngine.reloadCoverageTask;
        }

        [Test]
        public async Task Should_Invoke_Clean_Up_If_Provided()
        {
            bool invokedCleanUp = false;
            await Run_Cancelled_Coverage_Task(() =>
            {
                invokedCleanUp = true;
            });

            Assert.True(invokedCleanUp);
        }

        [Test]
        public async Task Should_Log_When_Coverage_Cancelled()
        {
            await Run_Cancelled_Coverage_Task();

            mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Cancelled.Message()));
            mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("Coverage cancelled", MessageContext.CoverageCancelled);
        }

        [Test]
        public async Task Should_Send_CoverageStoppedMessage_When_Cancelled()
        {
            await Run_Cancelled_Coverage_Task();

            mocker.GetMock<IEventAggregator>().AssertCoverageStopped(Times.Once());
        }

        private Task Vs_Shutting_Down_When_Running_Coverage_Task(Action cleanUp = null)
        {
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.SetupGet(disposeAwareTaskRunner => disposeAwareTaskRunner.IsVsShutdown).Returns(true);

            return Run_Cancelled_Coverage_Task(cleanUp);
        }

        [Test]
        public async Task Should_Not_Log_Coverage_Cancelled_When_Coverage_Task_Completes_And_Vs_Shutting_Down()
        {
            await Vs_Shutting_Down_When_Running_Coverage_Task();
            mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Cancelled.Message()), Times.Never());
        }

        [Test]
        public async Task Should_Not_Send_CoverageStoppedMessage_When_Coverage_Task_Completes_And_Vs_Shutting_Down()
        {
            await Vs_Shutting_Down_When_Running_Coverage_Task();
            mocker.GetMock<IEventAggregator>().AssertCoverageStopped(Times.Never());
        }

        [Test]
        public async Task Should_Not_Invoke_CleanUp_When_Coverage_Task_Completes_And_Vs_Shutting_Down()
        {
            var cleanUpInvoked = false;
            await Vs_Shutting_Down_When_Running_Coverage_Task(() =>
            {
                cleanUpInvoked = true;
            });

            Assert.False(cleanUpInvoked);
        }



    }

}