using System;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IDisposeAwareTaskRunner
    {
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        void RunAsync(Func<Task> taskProvider);
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        CancellationToken DisposalToken { get; }

        CancellationTokenSource CreateLinkedCancellationTokenSource();

        bool IsVsShutdown { get; }
    }
}
