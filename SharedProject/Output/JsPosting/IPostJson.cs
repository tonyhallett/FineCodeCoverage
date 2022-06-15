using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverage.Output.JsPosting
{
	internal interface IPostJson
	{
		void Ready(IJsonPoster jsonPoster, IWebViewImpl webViewImpl);
		void Refresh();
	}
}
