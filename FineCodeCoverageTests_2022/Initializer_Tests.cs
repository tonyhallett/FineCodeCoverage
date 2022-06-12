namespace FineCodeCoverageTests.Initializer_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Impl;
    using Moq;
    using NUnit.Framework;

    public class Initializer_Tests
    {
        private AutoMoqer mocker;
        private Initializer initializer;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.initializer = this.mocker.Create<Initializer>();
        }

        [Test]
        public void Should_Have_Initial_InitializeStatus_As_Initializing() =>
            Assert.That(this.initializer.InitializeStatus, Is.EqualTo(InitializeStatus.Initializing));

        [Test]
        public async Task Should_Log_Initializing_When_Initialize_Async()
        {
            await this.initializer.InitializeAsync(CancellationToken.None);
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
        public async Task Should_Not_Log_Failed_Initialization_When_Initialize_Cancelled_Async()
        {
            await this.initializer.InitializeAsync(CancellationTokenHelper.GetCancelledCancellationToken());
            this.mocker.Verify<ILogger>(l => l.Log("Failed Initialization", It.IsAny<Exception>()), Times.Never());
        }

        [Test]
        public async Task Should_Set_InitializeStatus_To_Initialized_When_Successfully_Completed_Async()
        {
            await this.initializer.InitializeAsync(CancellationToken.None);
            Assert.That(this.initializer.InitializeStatus, Is.EqualTo(InitializeStatus.Initialized));
        }

        [Test]
        public async Task Should_Log_Initialized_When_Successfully_Completed_Async()
        {
            await this.initializer.InitializeAsync(CancellationToken.None);
            this.mocker.Verify<ILogger>(l => l.Log("Initialized"));
        }

        [Test]
        public async Task Should_Initialize_Dependencies_In_Order_Async()
        {
            var disposalToken = CancellationToken.None;
            var callOrder = new List<int>();
            _ = this.mocker.GetMock<ICoverageProjectFactory>().Setup(cp => cp.Initialize()).Callback(() => callOrder.Add(1));
            _ = this.mocker.GetMock<IFCCEngine>().Setup(engine => engine.Initialize(this.initializer, disposalToken))
                .Callback(() => callOrder.Add(2));

            _ = this.mocker.GetMock<IPackageInitializer>().Setup(p => p.InitializeAsync(disposalToken))
                .Callback(() => callOrder.Add(3));

            await this.initializer.InitializeAsync(disposalToken);

            Assert.That(callOrder, Is.EqualTo(new List<int> { 1, 2, 3 }));
        }

        [Test]
        public async Task Should_Pass_Itself_To_FCCEngine_For_InitializeStatus_Async()
        {
            var disposalToken = CancellationToken.None;
            await this.initializer.InitializeAsync(disposalToken);
            this.mocker.Verify<IFCCEngine>(engine => engine.Initialize(this.initializer, disposalToken));
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
            var mockCoverageProjectFactory = this.mocker.GetMock<ICoverageProjectFactory>();
            _ = mockCoverageProjectFactory.Setup(
                coverageProjectFactory => coverageProjectFactory.Initialize()
                ).Throws(new Exception("The exception message"));

            await this.initializer.InitializeAsync(CancellationToken.None);

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

        private async Task InitializeWithExceptionAsync(Action<Exception> callback = null)
        {
            var initializeException = new Exception("initialize exception");
            _ = this.mocker.Setup<ICoverageProjectFactory>(a => a.Initialize()).Throws(initializeException);

            await this.initializer.InitializeAsync(CancellationToken.None);
            callback?.Invoke(initializeException);

        }

    }
}
