using Microsoft.Web.WebView2.Core;

namespace FineCodeCoverage.Output.WebView
{
	internal interface IWebViewController
	{
		void Initialize(IWebView webView);
		void CoreWebView2InitializationCompleted();
        void ProcessFailed(object coreWebView2ProcessFailedEventArgs);
    }
}
