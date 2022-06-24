namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Options;
    using FineCodeCoverageTests.Test_helpers;
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using Moq;
    using NUnit.Framework;

    internal abstract class TestContainerDiscoverer_Tests_Base
    {
        protected AutoMoqer Mocker { get; private set; }
        protected IAppOptions AppOptions { get; private set; }
        protected TestContainerDiscoverer TestContainerDiscoverer { get; private set; }
        protected Mock<IAppOptionsProvider> MockAppOptionsProvider { get; private set; }

        [SetUp]
        public void Setup()
        {
            this.Mocker = new AutoMoqer();
            this.Mocker.SetEmptyEnumerable<ITestInstantiationPathAware>();
            this.TestContainerDiscoverer = this.Mocker.Create<TestContainerDiscoverer>();
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            this.TestContainerDiscoverer.RunAsync = (asyncMethod) => asyncMethod().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            this.SetUpOptions();
        }

        protected void SetUpOptions(Action<Mock<IAppOptions>> setupAppOptions = null)
        {
            var mockAppOptions = new Mock<IAppOptions>();
            this.AppOptions = mockAppOptions.Object;
            _ = mockAppOptions.SetupGet(appOptions => appOptions.Enabled).Returns(true);
            setupAppOptions?.Invoke(mockAppOptions);

            this.MockAppOptionsProvider = this.Mocker.GetMock<IAppOptionsProvider>();

            _ = this.MockAppOptionsProvider.Setup(
                appOptionsProvider => appOptionsProvider.Provide()
            ).Returns(this.AppOptions);
        }

        #region assertions
        protected void Assert_MsCodeCoverage_Asked_If_Collecting(bool expectedCollecting) =>
            this.Mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverage => msCodeCoverage.IsCollectingAsync(It.IsAny<ITestOperation>()),
                expectedCollecting ? Times.Once() : Times.Never()
            );

        protected void Assert_Ms_Code_Coverage_Not_Collecting() =>
            this.Mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverage => msCodeCoverage.CollectAsync(It.IsAny<IOperation>(), It.IsAny<ITestOperation>()),
                Times.Never()
            );

        protected void Assert_Old_Style_Not_Collecting() =>
            this.Mocker.Verify<IOldStyleCoverage>(oldStyleCoverage =>
                oldStyleCoverage.CollectCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>()),
                Times.Never()
            );



        #endregion


        #region raise events
        protected void RaiseOperationStateChanged(TestOperationStates testOperationStates, IOperation operation = null)
        {
            var args = operation == null ? new OperationStateChangedEventArgs(testOperationStates) : new OperationStateChangedEventArgs(operation, (RequestStates)testOperationStates);
            this.Mocker.GetMock<IOperationState>().Raise(s => s.StateChanged += null, args);
        }

        protected void RaiseTestExecutionStarting(IOperation operation = null) =>
            this.RaiseOperationStateChanged(TestOperationStates.TestExecutionStarting, operation);

        protected void RaiseTestExecutionFinished(IOperation operation = null) =>
            this.RaiseOperationStateChanged(TestOperationStates.TestExecutionFinished, operation);

        protected void RaiseTestExecutionCancelling(IOperation operation = null) =>
            this.RaiseOperationStateChanged(TestOperationStates.TestExecutionCanceling, operation);

        protected void RaiseTestExecutionCancelAndFinished(IOperation operation = null) =>
            this.RaiseOperationStateChanged(TestOperationStates.TestExecutionCancelAndFinished, operation);

        protected void RaiseOperationSetFinished() =>
            this.RaiseOperationStateChanged(TestOperationStates.OperationSetFinished);
        #endregion
    }
}
