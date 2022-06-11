namespace FineCodeCoverageTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FineCodeCoverage.Core.Utilities.VsThreading;

    internal class TestThreadHelper : IThreadHelper
    {
        public IJoinableTaskFactory JoinableTaskFactory { get; } = new TestJoinableTaskFactory();

        public void ThrowIfNotOnUIThread()
        {

        }
    }

    internal class TestJoinableTaskFactory : IJoinableTaskFactory
    {
        public void Run(Func<Task> asyncMethod) =>
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            asyncMethod().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits


        public Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
