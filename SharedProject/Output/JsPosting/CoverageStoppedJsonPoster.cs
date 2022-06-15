using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.JsMessages;
using FineCodeCoverage.Output.WebView;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsPosting
{
    [Export(typeof(IPostJson))]
    internal class CoverageStoppedJsonPoster : IPostJson, IListener<CoverageStoppedMessage>
    {
        private IJsonPoster jsonPoster;

        [ImportingConstructor]
        public CoverageStoppedJsonPoster(IEventAggregator eventAggregator)
        {
            eventAggregator.AddListener(this);
        }

        public void Handle(CoverageStoppedMessage message)
        {
            jsonPoster.PostJson<object>("coverageStopped", null);
        }

        public void Ready(IJsonPoster jsonPoster, IWebViewImpl webViewImpl)
        {
            this.jsonPoster = jsonPoster;
        }

        public void Refresh()
        {

        }
    }

}
