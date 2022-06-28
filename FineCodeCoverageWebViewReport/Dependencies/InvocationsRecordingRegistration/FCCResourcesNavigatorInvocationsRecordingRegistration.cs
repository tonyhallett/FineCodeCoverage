using FineCodeCoverage.Output.HostObjects;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    internal class FCCResourcesNavigatorInvocationsRecordingRegistration : IWebViewHostObjectRegistration
    {
        public string Name => FCCResourcesNavigatorRegistration.HostObjectName;

        public object HostObject => new FCCResourcesNavigatorInvocationsHostObject();
    }
}
