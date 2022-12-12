using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    internal class FCCOutputPaneInvocationsRecordingRegistration : IWebViewHostObjectRegistration
    {
        public string Name => FCCOutputPaneHostObjectRegistration.HostObjectName;

        public object HostObject => new FCCOutputPaneInvocationsHostObject();

        public void InitializationCompleted(IWebViewInterface webViewInterface)
        {
        }
    }
}
