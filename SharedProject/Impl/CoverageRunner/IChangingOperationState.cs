using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;

namespace FineCodeCoverage.Impl
{
    internal interface IChangingOperationState
    {
        event EventHandler<OperationStateChangedEventArgs> OperationStateChanged;
    }

}
