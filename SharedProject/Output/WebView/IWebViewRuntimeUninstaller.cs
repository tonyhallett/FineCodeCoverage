using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.WebView
{
    internal interface IWebViewRuntimeUninstaller
    {
        Task SilentUninstallAsync(
            bool throwIfNotInstalled = false, 
            CancellationToken cancellationToken = default
        );
    }
}
