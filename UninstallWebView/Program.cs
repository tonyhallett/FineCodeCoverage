using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.WebView;
using System.Threading.Tasks;

namespace UninstallWebView
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await new WebViewRuntimeUninstaller(
                    new WebViewRuntimeRegistry(),
                    new ProcessUtil()
                ).SilentUninstallAsync();
            }
            /*
                catching as throwing as exit code is 19.
                See issue comment - is 19 a successful uninstall code
                https://github.com/MicrosoftEdge/WebView2Feedback/issues/2317#issuecomment-1165536731
            */
            catch { }
        }
    }
}
