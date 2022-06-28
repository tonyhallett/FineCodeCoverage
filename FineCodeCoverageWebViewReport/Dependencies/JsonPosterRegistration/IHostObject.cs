namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    public interface IHostObject
    {
#pragma warning disable IDE1006 // Naming Styles
        void postBack(string postBackObject);
#pragma warning restore IDE1006 // Naming Styles
        void SetJsonPoster(IRegJsonPoster jsonPoster);
    }
}
