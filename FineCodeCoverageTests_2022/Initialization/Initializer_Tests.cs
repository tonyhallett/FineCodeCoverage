namespace FineCodeCoverageTests.Initialization_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using Moq;
    using NUnit.Framework;
    using FineCodeCoverage.Core.Initialization;
    using System.Linq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.WebView;

    [TestFixture(true)]
    [TestFixture(false)]
    public class Initializer_Tests
    {
        private AutoMoqer mocker;
        private Initializer initializer;
        private List<Mock<IRequireInitialization>> mocksRequireInitialization;
        private Mock<IDisposeAwareTaskRunner> mockDisposeAwareTaskRunner;
        private readonly CancellationToken disposeAwareToken = CancellationToken.None;
        private readonly Action initialize;
        private readonly Action initializeOther;
        private readonly bool testExplorerInstantiation;
        private class OrderMetadata : IOrderMetadata
        {
            public OrderMetadata(int order) => this.Order = order;
            public int Order { get; }
        }

        public Initializer_Tests(bool initializedFromPackage)
        {
            this.testExplorerInstantiation = !initializedFromPackage;
            this.initialize = initializedFromPackage ?
                this.InitializeFromPackage : (Action)this.InitializeFromTestInstantiation;
            this.initializeOther = !initializedFromPackage ?
                this.InitializeFromPackage : (Action)this.InitializeFromTestInstantiation;
        }

        private void InitializeFromPackage() => this.initializer.PackageInitializing();

        private void InitializeFromTestInstantiation() => this.initializer.TestExplorerInstantion();

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mocksRequireInitialization = new List<Mock<IRequireInitialization>>
            {
                new Mock<IRequireInitialization>(),new Mock<IRequireInitialization>()
            };
            this.mocker.SetInstance(
                this.mocksRequireInitialization.Select(
                    (mockRequireInitialization, order) => new Lazy<IRequireInitialization, IOrderMetadata>(
                        () => mockRequireInitialization.Object,
                        new OrderMetadata(-order)
                    )
                )
            );
            this.mockDisposeAwareTaskRunner = this.mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = this.mockDisposeAwareTaskRunner.SetupGet(disposeAwareTaskRunner => disposeAwareTaskRunner.DisposalToken)
                .Returns(this.disposeAwareToken);
            _ = this.mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            ).Callback<Func<Task>>(taskProvider => taskProvider());
        }

        private async Task InitializeAsync()
        {
            this.initializer = this.mocker.Create<Initializer>();
            this.initialize();
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await this.initializer.initializeTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }

        private Task InitializeWithExceptionAsync(Action<Exception> exceptionCallback = null)
        {
            var exception = new Exception("initialize exception");
            exceptionCallback?.Invoke(exception);
            _ = this.mocksRequireInitialization[0].Setup(
                requiresInitialization => requiresInitialization.InitializeAsync(this.testExplorerInstantiation, this.disposeAwareToken)
            ).ThrowsAsync(exception);

            return this.InitializeAsync();
        }

        [Test]
        public async Task Should_Have_Initial_InitializeStatus_As_Initializing_Async()
        {
            var mockRequiresInitialization = this.mocksRequireInitialization[0];
            _ = mockRequiresInitialization.Setup(requireInitialization => requireInitialization.InitializeAsync(
                this.testExplorerInstantiation,
                It.IsAny<CancellationToken>())
            ).Callback(() => Assert.That(this.initializer.InitializeStatus, Is.EqualTo(InitializeStatus.Initializing)));

            await this.InitializeAsync();

            mockRequiresInitialization.VerifyAll();
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

            this.initializeOther();

            this.mockDisposeAwareTaskRunner.Verify(
                disposeAwareTaskRunner => disposeAwareTaskRunner.RunAsync(It.IsAny<Func<Task>>()),
                Times.Once()
            );
        }

        [Test]
        public void Should_Log_Initializing_When_Initialize()
        {
            _ = this.InitializeAsync();

            this.mocker.Verify<ILogger>(l => l.Log("Initializing"));
        }

        [Test]
        public async Task Should_Set_InitializeStatus_To_Error_If_Exception_When_Initialize_Async()
        {
            await this.InitializeWithExceptionAsync();

            Assert.That(this.initializer.InitializeStatus, Is.EqualTo(InitializeStatus.Error));
        }

        [Test]
        public async Task Should_Log_Failed_Initialization_With_Exception_if_Exception_When_Initialize_Async()
        {
            Exception initializeException = null;
            await this.InitializeWithExceptionAsync(exc => initializeException = exc);

            this.mocker.Verify<ILogger>(l => l.Log("Failed Initialization", initializeException));
        }

        [Test]
        public async Task Should_Initialize_All_That_IRequireInitialization_In_Order_Async()
        {
            var firstMockRequiresInitialization = this.mocksRequireInitialization[0];
            var secondMockRequiresInitialization = this.mocksRequireInitialization[1];

            var firstMockInitialized = false;
            _ = firstMockRequiresInitialization
                .Setup(requireInitialization => requireInitialization.InitializeAsync(this.testExplorerInstantiation, this.disposeAwareToken))
                .Callback(() => firstMockInitialized = true);

            _ = secondMockRequiresInitialization
                .Setup(requireInitialization => requireInitialization.InitializeAsync(this.testExplorerInstantiation, this.disposeAwareToken))
                .Callback(() => Assert.That(firstMockInitialized, Is.False));

            await this.InitializeAsync();

            firstMockRequiresInitialization.VerifyAll();
            secondMockRequiresInitialization.VerifyAll();
        }

        [Test]
        public void PackageInitializer_Should_Be_Initialized_First() =>
            MefOrderAssertions.TypeHasExpectedOrder(typeof(PackageInitializer), 0);

        [Test]
        public void WebViewRuntime_Should_Be_Initialized_Second() => MefOrderAssertions.TypeHasExpectedOrder(typeof(WebViewRuntime), 1);

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
    }
}
