using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IExecutionTimer))]
    internal class ExecutionTimer : IExecutionTimer
    {
        public TimeSpan Time(Action action)
        {
            var start = DateTime.Now;
            action();
            return DateTime.Now - start;
        }

        public async Task<TimeSpan> TimeAsync(Func<Task> task)
        {
            var start = DateTime.Now;
            await task();
            return DateTime.Now - start;
        }
    }
}
