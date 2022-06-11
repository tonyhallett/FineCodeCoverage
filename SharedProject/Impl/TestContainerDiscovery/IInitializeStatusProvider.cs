using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Impl
{
    internal interface IInitializeStatusProvider
    {
        InitializeStatus InitializeStatus { get; set; }
        string InitializeExceptionMessage { get; set; }

        Task WaitForInitializedAsync(CancellationToken cancellationToken);
        
    }

}

