using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Output.WebView;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsPosting
{
	[Export(typeof(IPostJson))]
	internal class LogMessageJsonPoster : IPostJson, IListener<LogMessage>
	{
		private IJsonPoster jsonPoster;
		public const string PostType = "message";

        public string Type => PostType;
		public const NotReadyPostBehaviour NotReadyBehaviour = NotReadyPostBehaviour.KeepAll;

		public NotReadyPostBehaviour NotReadyPostBehaviour => NotReadyBehaviour;

        [ImportingConstructor]
		public LogMessageJsonPoster(IEventAggregator eventAggregator)
		{
			eventAggregator.AddListener(this);
		}

		private void PostLogMessage(LogMessage message)
		{
			jsonPoster.PostJson(PostType, message);
		}

		public void Initialize(IJsonPoster jsonPoster)
        {
			this.jsonPoster = jsonPoster;
		}

		public void Handle(LogMessage message)
		{
			PostLogMessage(message);
		}

		public void Ready(IWebViewImpl webViewImpl)
		{
		}

		public void Refresh()
		{

		}
	}

}
