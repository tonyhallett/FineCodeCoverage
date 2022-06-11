using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IExecutionTimer
    {
        TimeSpan Time(Action action);
        Task<TimeSpan> TimeAsync(Func<Task> task);
    }
}
