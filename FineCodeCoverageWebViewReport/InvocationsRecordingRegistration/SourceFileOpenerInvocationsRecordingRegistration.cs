using FineCodeCoverage.Output.HostObjects;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    internal class SourceFileOpenerInvocationsRecordingRegistration : IWebViewHostObjectRegistration
    {
        public string Name => SourceFileOpenerHostObjectRegistration.HostObjectName;

        public object HostObject => new SourceFileOpenerInvocationsHostObject();
    }
}
