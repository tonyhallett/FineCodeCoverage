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

        public string Type => "coverageStopped";

        public NotReadyPostBehaviour NotReadyPostBehaviour => NotReadyPostBehaviour.KeepAll;

        public void Handle(CoverageStoppedMessage message)
        {
            jsonPoster.PostJson<object>(Type, null);
        }

        public void Initialize(IJsonPoster jsonPoster)
        {
            this.jsonPoster = jsonPoster;
        }

        public void Ready(IWebViewImpl webViewImpl)
        {
            
        }

        public void Refresh()
        {

        }
    }

}
