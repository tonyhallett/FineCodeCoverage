using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    internal interface IFirstTimeReportWindowOpener
    {
        Task OpenIfFirstTimeAsync(CancellationToken cancellationToken);
    }
}
