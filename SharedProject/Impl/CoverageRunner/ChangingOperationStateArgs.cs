using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;

namespace FineCodeCoverage.Impl
{
    internal class ChangingOperationStateArgs : EventArgs
    {
        public ChangingOperationStateArgs(TestOperationStates state, IOperation operation)
        {
            State = state;
            Operation = operation;
        }

        public TestOperationStates State { get; }
        public IOperation Operation { get; }

        public static ChangingOperationStateArgs Create(OperationStateChangedEventArgs originalArgs)
        {
            return new ChangingOperationStateArgs(originalArgs.State, originalArgs.Operation);
        }
    }

}
