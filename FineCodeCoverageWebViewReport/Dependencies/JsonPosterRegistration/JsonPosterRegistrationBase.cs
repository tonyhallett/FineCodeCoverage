using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.WebView;
using Newtonsoft.Json;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    internal abstract class JsonPosterBase : IPostJson
    {
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

    internal abstract class JsonPosterRegistrationBase<TData> : JsonPosterBase, IRegJsonPoster, IWebViewHostObjectRegistration
    {
        public abstract string Name { get; }
        private readonly IPostBackHostObject IHostObject = new PostBackHostObject();
        public object HostObject => IHostObject;

        public JsonPosterRegistrationBase()
        {
            IHostObject.SetJsonPoster(this);
        }
        public void PostJson(string data)
        {
            this.jsonPoster.PostJson(
                Type, 
                JsonConvert.DeserializeObject(data, typeof(TData), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                }));
        }
    }
}
