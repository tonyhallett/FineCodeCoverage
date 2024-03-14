using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    internal interface IPackageLoader
    {
        Task LoadPackageAsync(System.Threading.CancellationToken cancellationToken);
    }
}

