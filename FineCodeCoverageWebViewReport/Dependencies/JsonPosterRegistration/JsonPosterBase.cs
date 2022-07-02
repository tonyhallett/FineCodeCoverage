using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    internal abstract class JsonPosterBase : IPostJson {
        protected IJsonPoster jsonPoster;
        
        public abstract string Type { get; }
        public abstract NotReadyPostBehaviour NotReadyPostBehaviour { get; }


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

        public void PostJson(object postObject)
        {
            this.jsonPoster.PostJson(
                Type,
                postObject
            );
        }
    }
}
