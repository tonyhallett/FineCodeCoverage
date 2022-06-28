using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.WebView;
using Newtonsoft.Json;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    internal abstract class JsonPosterRegistrationBase<TData> : IWebViewHostObjectRegistration, IPostJson, IRegJsonPoster
    {
        protected IJsonPoster jsonPoster;

        public abstract string Name { get; }
        protected abstract string PostType { get; }

        public string Type => PostType;
        public NotReadyPostBehaviour NotReadyPostBehaviour => NotReadyPostBehaviour.KeepAll;

        private readonly IHostObject IHostObject = new HostObject();
        public object HostObject => IHostObject;

        public JsonPosterRegistrationBase()
        {
            IHostObject.SetJsonPoster(this);
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

        public void PostJson(string data)
        {
            this.jsonPoster.PostJson(
                PostType, 
                JsonConvert.DeserializeObject(data, typeof(TData), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                }));
        }
    }
}
