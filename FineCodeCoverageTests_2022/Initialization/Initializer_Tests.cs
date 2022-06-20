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

    public class Initializer_Tests
    {
        private AutoMoqer mocker;
        private Initializer initializer;
        private List<Mock<IRequireInitialization>> mocksRequireInitialization;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            var mockRequiresInitialization1 = new Mock<IRequireInitialization>();
            var mockRequiresInitialization2 = new Mock<IRequireInitialization>();
            var requiresInitialization = new List<IRequireInitialization>
            {
                mockRequiresInitialization1.Object,
                mockRequiresInitialization2.Object

            };
            this.mocksRequireInitialization = new List<Mock<IRequireInitialization>>
            {
                mockRequiresInitialization1,mockRequiresInitialization2
            };
            this.mocker.SetInstance<IEnumerable<IRequireInitialization>>(requiresInitialization);
            this.initializer = this.mocker.Create<Initializer>();
        }

        private Task InitializeWithExceptionAsync(Action<Exception> exceptionCallback = null)
        {
            var exception = new Exception("initialize exception");
            exceptionCallback?.Invoke(exception);
            var cancellationToken = CancellationToken.None;
            _ = this.mocksRequireInitialization[0].Setup(
                requiresInitialization => requiresInitialization.InitializeAsync(cancellationToken)
            ).ThrowsAsync(exception);

            return this.initializer.InitializeAsync(cancellationToken);
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
        public async Task Should_Initialize_All_That_IRequireInitialization_Async()
        {
            var cancellationToken = CancellationToken.None;
            await this.initializer.InitializeAsync(cancellationToken);

            this.mocksRequireInitialization.ForEach(
                mockRequireInitialization => mockRequireInitialization.Verify(
                    requireInitialization => requireInitialization.InitializeAsync(cancellationToken)
                )
            );
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
