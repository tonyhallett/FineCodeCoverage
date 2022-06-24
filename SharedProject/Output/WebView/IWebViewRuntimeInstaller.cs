using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.WebView
{
    internal interface IWebViewRuntimeInstaller
    {
        Task InstallAsync(CancellationToken cancellationToken, bool silent = true);
    }
}
