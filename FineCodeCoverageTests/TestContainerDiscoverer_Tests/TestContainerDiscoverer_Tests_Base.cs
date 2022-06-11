using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal abstract class TestContainerDiscoverer_Tests_Base
    {
        protected AutoMoqer mocker;
        protected IAppOptions appOptions;
        protected TestContainerDiscoverer testContainerDiscoverer;
        protected Mock<IAppOptionsProvider> mockAppOptionsProvider;

        [SetUp]
        public void Setup()
        {
            mocker = new AutoMoqer();
            testContainerDiscoverer = mocker.Create<TestContainerDiscoverer>();
            testContainerDiscoverer.RunAsync = (asyncMethod) =>
            {
                asyncMethod().Wait();
            };
            SetUpOptions();
            AdditionalSetup();
        }

        protected virtual void AdditionalSetup() { }

        protected void SetUpOptions(Action<Mock<IAppOptions>> setupAppOptions = null)
        {
            var mockAppOptions = new Mock<IAppOptions>();
            appOptions = mockAppOptions.Object;
            mockAppOptions.SetupGet(appOptions => appOptions.Enabled).Returns(true);
            setupAppOptions?.Invoke(mockAppOptions);
            
            mockAppOptionsProvider = mocker.GetMock<IAppOptionsProvider>();
            
            mockAppOptionsProvider.Setup(
                appOptionsProvider => appOptionsProvider.Get()
            ).Returns(appOptions);
        }

        #region assertions
        protected void Assert_MsCodeCoverage_Asked_If_Collecting(bool expectedCollecting)
        {
            mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverage => msCodeCoverage.IsCollectingAsync(It.IsAny<ITestOperation>()),
                expectedCollecting ? Times.Once() : Times.Never()
            );
        }

        protected void Assert_Ms_Code_Coverage_Not_Collecting()
        {
            mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverage => msCodeCoverage.CollectAsync(It.IsAny<IOperation>(), It.IsAny<ITestOperation>()),
                Times.Never()
            );
        }

        protected void Assert_Old_Style_Not_Collecting()
        {
            mocker.Verify<IOldStyleCoverage>(oldStyleCoverage =>
                oldStyleCoverage.CollectCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>()),
                Times.Never()
            );
        }

        

        #endregion


        #region raise events
        protected void RaiseOperationStateChanged(TestOperationStates testOperationStates, IOperation operation = null)
        {
            var args = operation == null ? new OperationStateChangedEventArgs(testOperationStates) : new OperationStateChangedEventArgs(operation, (RequestStates)testOperationStates);
            mocker.GetMock<IOperationState>().Raise(s => s.StateChanged += null, args);
        }

        protected void RaiseTestExecutionStarting(IOperation operation = null)
        {
            RaiseOperationStateChanged(TestOperationStates.TestExecutionStarting, operation);
        }

        protected void RaiseTestExecutionFinished(IOperation operation = null)
        {
            RaiseOperationStateChanged(TestOperationStates.TestExecutionFinished, operation);
        }

        protected void RaiseTestExecutionCancelling(IOperation operation = null)
        {
            RaiseOperationStateChanged(TestOperationStates.TestExecutionCanceling, operation);
        }

        protected void RaiseTestExecutionCancelAndFinished(IOperation operation = null)
        {
            RaiseOperationStateChanged(TestOperationStates.TestExecutionCancelAndFinished, operation);
        }

        protected void RaiseOperationSetFinished()
        {
            RaiseOperationStateChanged(TestOperationStates.OperationSetFinished);
        }
        #endregion
    }
}