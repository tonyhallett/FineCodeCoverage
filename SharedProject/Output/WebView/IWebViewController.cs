namespace FineCodeCoverage.Output.WebView
{
	internal interface IWebViewController
	{
		void Initialize(IWebView webView);
		void CoreWebView2InitializationCompleted();
        void ProcessFailed(object coreWebView2ProcessFailedEventArgs);
		string UserDataFolder { get; }
		string AdditionalBrowserArguments { get; }
		IWebViewSettings WebViewSettings { get; }
	}
}
