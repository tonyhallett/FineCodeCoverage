using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.JsSerialization;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    internal class StylingJsonPosterRegistration : JsonPosterRegistrationBase<Styling>
    {
        public const string RegistrationName = "styling";
       
        public override string Name => RegistrationName;

        protected override string PostType => StylingJsonPoster.PostType;
    }
}
