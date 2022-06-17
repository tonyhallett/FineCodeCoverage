using FineCodeCoverage.Output.HostObjects;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    internal class FCCOutputPaneInvocationsRecordingRegistration : IWebViewHostObjectRegistration
    {
        public string Name => FCCOutputPaneHostObjectRegistration.HostObjectName;

        public object HostObject => new FCCOutputPaneInvocationsHostObject();
    }
}
