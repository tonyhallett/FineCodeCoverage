using System.Runtime.InteropServices;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class PostBackHostObject : IPostBackHostObject
    {
        private IRegJsonPoster jsonPoster;

        public void postBack(string postBackObject)
        {
            jsonPoster.PostJson(postBackObject);
        }

        public void SetJsonPoster(IRegJsonPoster jsonPoster)
        {
            this.jsonPoster = jsonPoster;
        }
    }
}
