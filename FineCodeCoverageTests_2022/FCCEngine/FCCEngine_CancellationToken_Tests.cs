namespace FineCodeCoverageTests.FCCEngine_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;
    using FineCodeCoverage.Core.Initialization;

    public class FCCEngine_CancellationToken_Tests
    {
        private AutoMoqer mocker;
        private Mock<IInitializeStatusProvider> mockInitializeStatusProvider;
        private FCCEngine fccEngine;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mockInitializeStatusProvider = new Mock<IInitializeStatusProvider>();
            this.mocker.SetInstance(new Lazy<IInitializeStatusProvider>(() => this.mockInitializeStatusProvider.Object));
            this.fccEngine = this.mocker.Create<FCCEngine>();
        }

        [Test]
        public void Should_Cancel_The_Vs_Linked_CancellationTokenSource_When_StopCoverage()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            this.fccEngine.cancellationTokenSource = cancellationTokenSource;
            this.fccEngine.StopCoverage();

            Assert.That(cancellationTokenSource.IsCancellationRequested, Is.True);
        }

        [Test]
        public void Should_Not_Throw_If_No_CancellationTokenSource_When_StopCoverage() => this.fccEngine.StopCoverage();

        [Test]
        public void Should_Not_Throw_If_CancellationTokenSource_Is_Disposed_When_StopCoverage()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            this.fccEngine.cancellationTokenSource = cancellationTokenSource;
            cancellationTokenSource.Dispose();
            this.fccEngine.StopCoverage();
        }

        [Test]
        public void Should_Cancel_The_Vs_Linked_CancellationTokenSource_When_RunCancellableCoverageTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            this.fccEngine.cancellationTokenSource = cancellationTokenSource;

            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(new CancellationTokenSource());

            this.fccEngine.RunCancellableCoverageTask((_) => Task.FromResult(new List<CoverageLine>()), null);

            Assert.That(cancellationTokenSource.IsCancellationRequested, Is.True);
        }

        [Test]
        public void Should_Set_A_New_Vs_Linked_CancellationTokenSource_When_RunCancellableCoverageTask()
        {
            var vsLinkedCancellationTokenSource = new CancellationTokenSource();
            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(vsLinkedCancellationTokenSource);

            this.fccEngine.RunCancellableCoverageTask((_) => Task.FromResult(new List<CoverageLine>()), null);

            Assert.Multiple(() =>
            {
                Assert.That(this.fccEngine.cancellationTokenSource, Is.SameAs(vsLinkedCancellationTokenSource));
                Assert.That(vsLinkedCancellationTokenSource.IsCancellationRequested, Is.False);
            });
        }

        [Test]
        public void Should_Run_A_Dispose_Aware_Task_When_RunCancellableCoverageTask()
        {
            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(new CancellationTokenSource());

            this.fccEngine.RunCancellableCoverageTask((_) => Task.FromResult(new List<CoverageLine>()), null);

            mockDisposeAwareTaskRunner.Verify(disposeAwareTaskRunner => disposeAwareTaskRunner.RunAsync(It.IsAny<Func<Task>>()));
        }

        [Test]
        public async Task Should_Pass_The_Vs_Shutdown_Linked_CancellationToken_To_The_ReportResultProvider_When_RunCancellableCoverageTask_Async()
        {
            var linkedCancellationTokenSource = new CancellationTokenSource();
            var linkedCancellationToken = linkedCancellationTokenSource.Token;

            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(linkedCancellationTokenSource);
            _ = mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider => taskProvider());

            _ = mockInitializeStatusProvider.Setup(
                initializeStatusProvider => initializeStatusProvider.WaitForInitializedAsync(It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var cancellableCoverageTaskCancellationToken = CancellationToken.None;
            this.fccEngine.RunCancellableCoverageTask((ct) =>
            {
                cancellableCoverageTaskCancellationToken = ct;
                return Task.FromResult(new List<CoverageLine>());
            }, null);

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await this.fccEngine.reloadCoverageTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

            Assert.That(cancellableCoverageTaskCancellationToken, Is.EqualTo(linkedCancellationToken));
        }

        [Test]
        public async Task Should_WaitForInitializedAsync_With_the_Vs_Linked_CancellationToken_When_RunCancellableCoverageTask_Async()
        {
            var vsLinkedCancellationTokenSource = new CancellationTokenSource();
            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(vsLinkedCancellationTokenSource);

            _ = mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider => taskProvider());

            _ = this.mockInitializeStatusProvider.Setup(
                initializeStatusProvider => initializeStatusProvider.WaitForInitializedAsync(vsLinkedCancellationTokenSource.Token)
            ).Returns(Task.CompletedTask);

            this.fccEngine.RunCancellableCoverageTask((_) => Task.FromResult(new List<CoverageLine>()), null);

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await this.fccEngine.reloadCoverageTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
            this.mockInitializeStatusProvider.VerifyAll();
        }

        [Test]
        public async Task Should_Dispose_CancellationTokenSource_For_That_Coverage_Task_When_Completes_Async()
        {
            var laterRunCancellationTokenSource = new CancellationTokenSource();
            var currentRunCancellationTokenSource = new CancellationTokenSource();

            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(currentRunCancellationTokenSource);
            _ = mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            )
            .Callback<Func<Task>>(taskProvider =>
            {
                this.fccEngine.cancellationTokenSource = laterRunCancellationTokenSource;
                _ = taskProvider();
            });

            _ = this.mockInitializeStatusProvider.Setup(
                initializeStatusProvider => initializeStatusProvider.WaitForInitializedAsync(It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            this.fccEngine.RunCancellableCoverageTask((_) => Task.FromResult(new List<CoverageLine>()), null);

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await this.fccEngine.reloadCoverageTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

            Assert.Multiple(() =>
            {
                Assert.That(currentRunCancellationTokenSource.IsDisposed(), Is.True);
                Assert.That(laterRunCancellationTokenSource.IsDisposed(), Is.False);
            });
        }

        [Test]
        public async Task Should_Invoke_Clean_Up_If_Provided_Async()
        {
            var invokedCleanUp = false;
            await this.Run_Cancelled_Coverage_Task_Async(() => invokedCleanUp = true);

            Assert.That(invokedCleanUp, Is.True);
        }

        [Test]
        public async Task Should_Log_When_Coverage_Cancelled_Async()
        {
            await this.Run_Cancelled_Coverage_Task_Async();

            this.mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Cancelled.Message()));
            this.mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("Coverage cancelled", MessageContext.CoverageCancelled);
        }

        [Test]
        public async Task Should_Send_CoverageStoppedMessage_When_Cancelled_Async()
        {
            await this.Run_Cancelled_Coverage_Task_Async();

            this.mocker.GetMock<IEventAggregator>().AssertCoverageStopped(Times.Once());
        }

        [Test]
        public async Task Should_Not_Log_Coverage_Cancelled_When_Coverage_Task_Completes_And_Vs_Shutting_Down_Async()
        {
            await this.Vs_Shutting_Down_When_Running_Coverage_Task_Async();
            this.mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Cancelled.Message()), Times.Never());
        }

        [Test]
        public async Task Should_Not_Send_CoverageStoppedMessage_When_Coverage_Task_Completes_And_Vs_Shutting_Down_Async()
        {
            await this.Vs_Shutting_Down_When_Running_Coverage_Task_Async();
            this.mocker.GetMock<IEventAggregator>().AssertCoverageStopped(Times.Never());
        }

        [Test]
        public async Task Should_Not_Invoke_CleanUp_When_Coverage_Task_Completes_And_Vs_Shutting_Down_Async()
        {
            var cleanUpInvoked = false;
            await this.Vs_Shutting_Down_When_Running_Coverage_Task_Async(() => cleanUpInvoked = true);

            Assert.That(cleanUpInvoked, Is.False);
        }

        private Task Vs_Shutting_Down_When_Running_Coverage_Task_Async(Action cleanUp = null)
        {
            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.SetupGet(disposeAwareTaskRunner => disposeAwareTaskRunner.IsVsShutdown).Returns(true);

            return this.Run_Cancelled_Coverage_Task_Async(cleanUp);
        }

        private async Task Run_Cancelled_Coverage_Task_Async(Action cleanUp = null)
        {
            var cancelledCancellationTokenSource = new CancellationTokenSource();
            cancelledCancellationTokenSource.Cancel();

            var mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.CreateLinkedCancellationTokenSource()).Returns(cancelledCancellationTokenSource);
            _ = mockDisposeAwareTaskRunner.Setup(runner => runner.RunAsync(It.IsAny<Func<Task>>())).Callback<Func<Task>>(taskProvider => taskProvider());

            this.fccEngine.RunCancellableCoverageTask((_) => Task.FromResult(new List<CoverageLine>()), cleanUp);
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await this.fccEngine.reloadCoverageTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }

    }

}
