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
        private bool earlyCoverageStopped;

        [ImportingConstructor]
        public CoverageStoppedJsonPoster(IEventAggregator eventAggregator)
        {
            eventAggregator.AddListener(this);
        }

        public void Handle(CoverageStoppedMessage message)
        {
            if (jsonPoster != null)
            {
                PostJson();
            }
            else
            {
                earlyCoverageStopped = true;
            }
            
        }

        private void PostJson()
        {
            jsonPoster.PostJson<object>("coverageStopped", null);
        }

        public void Ready(IJsonPoster jsonPoster, IWebViewImpl webViewImpl)
        {
            this.jsonPoster = jsonPoster;
            if (earlyCoverageStopped)
            {
                earlyCoverageStopped = false;
                PostJson();
            }
        }

        public void Refresh()
        {

        }
    }

}
