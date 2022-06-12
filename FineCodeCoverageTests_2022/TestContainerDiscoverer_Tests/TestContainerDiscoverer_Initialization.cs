namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Impl;
    using Moq;
    using NUnit.Framework;

    internal class TestContainerDiscoverer_Initialization
    {
        [Test]
        public async Task Should_Initialize_Vs_Shutdown_Aware_Async()
        {
            var mocker = new AutoMoqer();

            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            _ = mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            ).Callback<Func<Task>>(taskProvider => taskProvider());

            var vsShutdownCancellationToken = CancellationToken.None;
            _ = mockDisposeAwareTaskRunner.SetupGet(
                disposeAwareTaskRunner => disposeAwareTaskRunner.DisposalToken
            ).Returns(vsShutdownCancellationToken);

            var testContainerDiscoverer = mocker.Create<TestContainerDiscoverer>();
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await testContainerDiscoverer.initializeTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

            mocker.Verify<IInitializer>(i => i.InitializeAsync(vsShutdownCancellationToken));
        }

    }
}
