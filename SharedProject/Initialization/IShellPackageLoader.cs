using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    internal interface IShellPackageLoader
    {
        Task LoadPackageAsync();
    }
}
