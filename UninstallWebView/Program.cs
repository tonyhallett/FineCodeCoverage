using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.WebView;
using System.Threading.Tasks;

namespace UninstallWebView
{
    internal class Program
    {
        static Task Main(string[] args) => 
            new WebViewRuntimeUninstaller(
                new WebViewRuntimeRegistry(),
                new ProcessUtil()
            ).SilentUninstallAsync(false, true);
    }
}
