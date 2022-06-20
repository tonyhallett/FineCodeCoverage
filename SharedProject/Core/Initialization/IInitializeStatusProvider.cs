using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Initialization
{
    internal interface IInitializeStatusProvider
    {
        InitializeStatus InitializeStatus { get; set; }
        string InitializeExceptionMessage { get; set; }

        Task WaitForInitializedAsync(CancellationToken cancellationToken);
        
    }

}

