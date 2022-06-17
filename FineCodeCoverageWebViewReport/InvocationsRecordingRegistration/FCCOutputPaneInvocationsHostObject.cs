using FineCodeCoverage.Output.HostObjects;
using System.Runtime.InteropServices;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FCCOutputPaneInvocationsHostObject : InvocationsRecordingHostObject, IFCCOutputPaneHostObject
    {
        public void show()
        {
            AddInvocation(nameof(show));
        }
    }
}
