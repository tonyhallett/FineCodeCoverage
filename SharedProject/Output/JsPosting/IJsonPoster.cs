namespace FineCodeCoverage.Output.JsPosting
{
    internal interface IJsonPoster
    {
        void PostJson<T>(string type, T data);

    }
}
