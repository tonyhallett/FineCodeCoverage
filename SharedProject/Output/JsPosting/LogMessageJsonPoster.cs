using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Output.WebView;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsPosting
{
	[Export(typeof(IPostJson))]
	internal class LogMessageJsonPoster : IPostJson, IListener<LogMessage>
	{
		private readonly List<LogMessage> earlyLogMessages = new List<LogMessage>();
		private IJsonPoster jsonPoster;
		private bool ready;
		public const string PostType = "message";

		[ImportingConstructor]
		public LogMessageJsonPoster(IEventAggregator eventAggregator)
		{
			eventAggregator.AddListener(this);
		}

		private void PostLogMessage(LogMessage message)
		{
			jsonPoster.PostJson(PostType, message);
		}

		private void PostEarlyLogMessages()
		{
			earlyLogMessages.ForEach(logMessage => PostLogMessage(logMessage));
			earlyLogMessages.Clear();
		}

		public void Handle(LogMessage message)
		{
			if (ready)
			{
				PostLogMessage(message);
			}
			else
			{
				earlyLogMessages.Add(message);
			}

		}

		public void Ready(IJsonPoster jsonPoster, IWebViewImpl webViewImpl)
		{
			this.jsonPoster = jsonPoster;
			ready = true;
			PostEarlyLogMessages();
		}

		public void Refresh()
		{

		}
	}

}
