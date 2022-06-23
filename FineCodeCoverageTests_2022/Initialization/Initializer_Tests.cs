namespace FineCodeCoverageTests.Initialization_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Engine;
    using Moq;
    using NUnit.Framework;
    using FineCodeCoverage.Core.Initialization;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Output;
    using System.Linq;
    using FineCodeCoverage.Core.Utilities;

    [TestFixture(true)]
    [TestFixture(false)]
    public class Initializer_Tests
    {
        private AutoMoqer mocker;
        private Initializer initializer;
        private List<Mock<IRequireInitialization>> mocksRequireInitialization;
        private Mock<IDisposeAwareTaskRunner> mockDisposeAwareTaskRunner;
        private readonly Action initializeNotify;
        private readonly Action otherInitializeNotify;
        public Initializer_Tests(bool testInstantiationAware)
        {
            if (testInstantiationAware)
            {
                this.initializeNotify = this.InitializeTestInstantiationPathAware;
                this.otherInitializeNotify = this.InitializePackageInitializeAware;
            }
            else
            {
                this.initializeNotify = this.InitializePackageInitializeAware;
                this.otherInitializeNotify = this.InitializeTestInstantiationPathAware;
            }
        }

        private void InitializeTestInstantiationPathAware() =>
            (this.initializer as ITestInstantiationPathAware).Notify(null);

        private void InitializePackageInitializeAware() =>
            (this.initializer as IPackageInitializeAware).Notify();

        private readonly CancellationToken disposeAwareToken = CancellationToken.None;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mocksRequireInitialization = new List<Mock<IRequireInitialization>>
            {
                new Mock<IRequireInitialization>(),new Mock<IRequireInitialization>()
            };
            this.mocker.SetInstance(
                this.mocksRequireInitialization.Select(mockRequireInitialization => mockRequireInitialization.Object)
            );
            this.mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = this.mockDisposeAwareTaskRunner.SetupGet(disposeAwareTaskRunner => disposeAwareTaskRunner.DisposalToken)
                .Returns(this.disposeAwareToken);
            _ = this.mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            ).Callback<Func<Task>>(taskProvider => taskProvider());

            this.initializer = this.mocker.Create<Initializer>();
        }

        private Task InitializeWithExceptionAsync(Action<Exception> exceptionCallback = null)
        {
            var exception = new Exception("initialize exception");
            exceptionCallback?.Invoke(exception);
            _ = this.mocksRequireInitialization[0].Setup(
                requiresInitialization => requiresInitialization.InitializeAsync(this.disposeAwareToken)
            ).ThrowsAsync(exception);

            return this.InitializeAsync();
        }

        [Test]
        public void Should_Have_Initial_InitializeStatus_As_Initializing() =>
            Assert.That(this.initializer.InitializeStatus, Is.EqualTo(InitializeStatus.Initializing));

        private async Task InitializeAsync()
        {
            this.initializeNotify();
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await this.initializer.initializeTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }

        [Test]
        public async Task Should_Initialize_With_The_DisposeAwareTaskRunner_Async()
        {
            await this.InitializeAsync();

            this.mockDisposeAwareTaskRunner.VerifyAll();
        }

        [Test]
        public async Task Should_Initialize_Once_Async()
        {
            await this.InitializeAsync();

            this.otherInitializeNotify();

            this.mockDisposeAwareTaskRunner.Verify(
                disposeAwareTaskRunner => disposeAwareTaskRunner.RunAsync(It.IsAny<Func<Task>>()),
                Times.Once()
            );
        }

        [Test]
        public async Task Should_Log_Initializing_When_Initialize_Async()
        {
            await this.InitializeAsync();
            this.mocker.Verify<ILogger>(l => l.Log("Initializing"));
        }

        [Test]
        public async Task Should_Set_InitializeStatus_To_Error_If_Exception_When_Initialize_Async()
        {
            await this.InitializeWithExceptionAsync();

            Assert.That(this.initializer.InitializeStatus, Is.EqualTo(InitializeStatus.Error));
        }

        [Test]
        public async Task Should_Set_InitializeExceptionMessage_If_Exception_When_Initialize_Async()
        {
            await this.InitializeWithExceptionAsync();

            Assert.That(this.initializer.InitializeExceptionMessage, Is.EqualTo("initialize exception"));
        }

        [Test]
        public async Task Should_Log_Failed_Initialization_With_Exception_if_Exception_When_Initialize_Async()
        {
            Exception initializeException = null;
            await this.InitializeWithExceptionAsync(exc => initializeException = exc);

            this.mocker.Verify<ILogger>(l => l.Log("Failed Initialization", initializeException));
        }

        [Test]
        public async Task Should_Initialize_All_That_IRequireInitialization_Async()
        {
            await this.InitializeAsync();

            this.mocksRequireInitialization.ForEach(
                mockRequireInitialization => mockRequireInitialization.Verify(
                    requireInitialization => requireInitialization.InitializeAsync(this.disposeAwareToken)
                )
            );
        }

        [Test]
        public async Task Should_Not_Log_Failed_Initialization_When_Initialize_Cancelled_Async()
        {
            _ = this.mockDisposeAwareTaskRunner.SetupGet(disposeAwareTaskRunner => disposeAwareTaskRunner.DisposalToken)
                .Returns(CancellationTokenHelper.GetCancelledCancellationToken());
            await this.InitializeAsync();

            this.mocker.Verify<ILogger>(l => l.Log("Failed Initialization", It.IsAny<Exception>()), Times.Never());
        }

        [Test]
        public async Task Should_Set_InitializeStatus_To_Initialized_When_Successfully_Completed_Async()
        {
            await this.InitializeAsync();

            Assert.That(this.initializer.InitializeStatus, Is.EqualTo(InitializeStatus.Initialized));
        }

        [Test]
        public async Task Should_Log_Initialized_When_Successfully_Completed_Async()
        {
            await this.InitializeAsync();

            this.mocker.Verify<ILogger>(l => l.Log("Initialized"));
        }

        [Test]
        public void Should_ThrowIfCancellationRequested_When_WaitForInitialized()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            _ = Assert.ThrowsAsync<OperationCanceledException>(
                async () => await this.initializer.WaitForInitializedAsync(cancellationTokenSource.Token)
            );
        }

        [Test]
        public async Task Should_Throw_If_InitializationFailed_When_WaitForInitialized_Async()
        {
            await this.InitializeWithExceptionAsync();

            _ = Assert.ThrowsAsync<Exception>(
                async () => await this.initializer.WaitForInitializedAsync(CancellationToken.None),
                "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.  The exception message"
                );
        }

        [Test]
        public async Task Should_WaitForInitializedAsync_Logging_If_Initializing_Async()
        {
            var times = 5;
            this.initializer.initializeWait = 100;
            var waitForInitializedTask = this.initializer.WaitForInitializedAsync(CancellationToken.None);

            var setInitializedTask = Task.Delay(times * this.initializer.initializeWait)
                .ContinueWith(
                    _ => this.initializer.InitializeStatus = InitializeStatus.Initialized,
                    TaskScheduler.Current
                );

            await Task.WhenAll(waitForInitializedTask, setInitializedTask);

            this.mocker.Verify<ILogger>(l => l.Log(CoverageStatus.Initializing.Message()), Times.AtLeast(times));
        }

    }
}
