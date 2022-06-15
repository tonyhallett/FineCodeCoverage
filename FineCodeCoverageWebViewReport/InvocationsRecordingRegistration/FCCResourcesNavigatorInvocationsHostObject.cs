using FineCodeCoverage.Output.HostObjects;
using System.Runtime.InteropServices;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FCCResourcesNavigatorInvocationsHostObject : InvocationsRecordingHostObject, IFCCResourcesNavigatorHostObject
    {
        public void buyMeACoffee()
        {
            AddInvocation(nameof(buyMeACoffee));
        }

        public void logIssueOrSuggestion()
        {
            AddInvocation(nameof(logIssueOrSuggestion));
        }

        public void rateAndReview()
        {
            AddInvocation(nameof(rateAndReview));
        }

        public void readReadMe()
        {
            AddInvocation(nameof(readReadMe));
        }
    }
}
