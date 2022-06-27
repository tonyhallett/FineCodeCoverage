using Microsoft.Web.WebView2.Core;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Output.WebView
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWebViewRuntimeInstallationChecker))]
    internal class WebViewRuntimeInstallationChecker : IWebViewRuntimeInstallationChecker
    {
        private readonly IWebViewRuntimeRegistry webViewRuntimeRegistry;

        [ImportingConstructor]
        public WebViewRuntimeInstallationChecker(
            IWebViewRuntimeRegistry webViewRuntimeRegistry    
        )
        {
            this.webViewRuntimeRegistry = webViewRuntimeRegistry;
        }

        public bool IsInstalled()
        {
            var isInstalled = webViewRuntimeRegistry.GetEntries() != null;
            return isInstalled;
        }
    }
}
