using FineCodeCoverage.Core.Initialization;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(ITestInstantiationPathAware))]
    internal class ChangingOperationStateWhenInitialized : ITestInstantiationPathAware, IChangingOperationState
    {
        private readonly IInitializeStatusProvider initializeStatusProvider;
        private bool initialized;
        public event EventHandler<OperationStateChangedEventArgs> OperationStateChanged;

        [ImportingConstructor]
        public ChangingOperationStateWhenInitialized(
            [Import(typeof(IOperationState))]
            IOperationState operationState,
            IInitializeStatusProvider initializeStatusProvider,
            IOperationStateChangedHandler operationStateChangedHandler
        )
        {
            this.initializeStatusProvider = initializeStatusProvider;
            operationStateChangedHandler.Initialize(this);
            operationState.StateChanged += OperationState_StateChanged;
        }

        private void OperationState_StateChanged(object sender, OperationStateChangedEventArgs operationStateChangedEventArgs)
        {
            if (operationStateChangedEventArgs.State == TestOperationStates.TestExecutionStarting)
            {
                initialized = initializeStatusProvider.InitializeStatus == InitializeStatus.Initialized;
            }

            if (initialized)
            {
                OperationStateChanged.Invoke(this, operationStateChangedEventArgs);
            }
        }

        public void TestExplorerInstantion()
        {
        }
    }

}
