using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    internal class SourceFileOpenerInvocationsRecordingRegistration : IWebViewHostObjectRegistration
    {
        public string Name => SourceFileOpenerHostObjectRegistration.HostObjectName;

        public object HostObject => new SourceFileOpenerInvocationsHostObject();

        public void InitializationCompleted(IWebViewInterface webViewInterface)
        {
        }
    }
}
