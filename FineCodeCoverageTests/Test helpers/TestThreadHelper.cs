﻿using FineCodeCoverage.Core.Utilities.VsThreading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverageTests
{
    internal class TestThreadHelper : IThreadHelper
    {
        public IJoinableTaskFactory JoinableTaskFactory { get; } = new TestJoinableTaskFactory();

        public void ThrowIfNotOnUIThread()
        {
            
        }
    }

    internal class TestJoinableTaskFactory : IJoinableTaskFactory
    {
        public void Run(Func<Task> asyncMethod)
        {
            asyncMethod().Wait();
        }

        public Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
