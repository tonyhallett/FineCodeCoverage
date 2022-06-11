using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestContainerDiscoverer_Initialization
    {
        [Test]
        public void Should_Initialize_Vs_Shutdown_Aware()
        {
            var mocker = new AutoMoqer();

            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
            mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsync(It.IsAny<Func<Task>>())
            ).Callback<Func<Task>>(async taskProvider => await taskProvider());

            var vsShutdownCancellationToken = CancellationToken.None;
            mockDisposeAwareTaskRunner.SetupGet(
                disposeAwareTaskRunner => disposeAwareTaskRunner.DisposalToken
            ).Returns(vsShutdownCancellationToken);

            var testContainerDiscoverer = mocker.Create<TestContainerDiscoverer>();
            testContainerDiscoverer.initializeTask.Wait();
            
            mocker.Verify<IInitializer>(i => i.InitializeAsync(vsShutdownCancellationToken));
        }

    }
}