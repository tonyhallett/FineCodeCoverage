using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    internal class FCCResourcesNavigatorInvocationsRecordingRegistration : IWebViewHostObjectRegistration
    {
        public string Name => FCCResourcesNavigatorRegistration.HostObjectName;

        public object HostObject => new FCCResourcesNavigatorInvocationsHostObject();

        public void InitializationCompleted(IWebViewInterface webViewInterface)
        {
        }
    }
}
