using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverage.Output.JsPosting
{
	internal enum NotReadyPostBehaviour { Forget, KeepLast, KeepAll}
	internal interface IPostJson
	{
		string Type { get; }
		NotReadyPostBehaviour NotReadyPostBehaviour { get; }
		void Initialize(IJsonPoster jsonPoster);
		void Ready(IWebViewImpl webViewImpl);
		void Refresh();
	}
}
