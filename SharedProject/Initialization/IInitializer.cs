using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    internal interface IInitializer : IInitializeStatusProvider
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }

}

