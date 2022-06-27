using System;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IDisposeAwareTaskRunner
    {
        Task RunAsync(Func<Task> taskProvider);
        CancellationToken DisposalToken { get; }

        CancellationTokenSource CreateLinkedCancellationTokenSource();

        bool IsVsShutdown { get; }
    }
}
