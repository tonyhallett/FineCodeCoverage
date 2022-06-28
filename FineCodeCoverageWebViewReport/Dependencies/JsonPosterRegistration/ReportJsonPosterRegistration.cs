using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.JsSerialization.ReportGenerator;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    internal class ReportJsonPosterRegistration : JsonPosterRegistrationBase<Report>
    {
        public const string RegistrationName = "report";

        public override string Name => RegistrationName;

        public override NotReadyPostBehaviour NotReadyPostBehaviour => ReportJsonPoster.NotReadyBehaviour;

        public override string Type => ReportJsonPoster.PostType;
    }
}
