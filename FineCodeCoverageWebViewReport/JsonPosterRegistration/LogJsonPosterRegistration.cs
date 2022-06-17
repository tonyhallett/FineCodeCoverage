using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Output.JsPosting;

namespace FineCodeCoverageWebViewReport.JsonPosterRegistration
{
    internal class LogJsonPosterRegistration : JsonPosterRegistrationBase<LogMessage>
    {
        public const string RegistrationName = "log";

        public override string Name => RegistrationName;

        protected override string PostType => LogMessageJsonPoster.PostType;
    }
}
