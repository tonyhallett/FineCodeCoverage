using FineCodeCoverage.Output.HostObjects;
using System.Runtime.InteropServices;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class SourceFileOpenerInvocationsHostObject : InvocationsRecordingHostObject, ISourceFileOpenerHostObject
    {
        public void openAtLine(string filePath, int line)
        {
            AddInvocation(nameof(openAtLine), filePath, line);
        }

        public void openFiles(object[] filePaths)
        {
            AddInvocation(nameof(openFiles), filePaths);
        }
    }
}
