using Microsoft.Web.WebView2.Core;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Output.WebView
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWebViewRuntimeInstallationChecker))]
    internal class WebViewRuntimeInstallationChecker : IWebViewRuntimeInstallationChecker
    {
        public bool IsInstalled()
        {
            var installed = false;
            //     Gets the browser version info including channel name if it is not the stable
            //     channel or WebView2 Runtime.

            //   T:Microsoft.Web.WebView2.Core.WebView2RuntimeNotFoundException:
            //     WebView2 Runtime installation is missing.
            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
                installed = true;
            }
            catch (WebView2RuntimeNotFoundException) { }

            return installed;
        }
    }
}
