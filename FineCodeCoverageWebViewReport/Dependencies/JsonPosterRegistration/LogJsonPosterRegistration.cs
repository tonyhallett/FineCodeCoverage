using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Output.JsPosting;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    internal class LogJsonPosterRegistration : JsonPosterRegistrationBase<LogMessage>
    {
        public const string RegistrationName = "log";

        public override string Name => RegistrationName;

        public override NotReadyPostBehaviour NotReadyPostBehaviour => LogMessageJsonPoster.NotReadyBehaviour;

        public override string Type => LogMessageJsonPoster.PostType;
    }
}
