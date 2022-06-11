using FineCodeCoverage.Core.Utilities;
using System.Runtime.InteropServices;

namespace FineCodeCoverage.Output.HostObjects
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FCCResourcesNavigatorHostObject
    {
        private const string payPal = "https://paypal.me/FortuneNgwenya";

        private const string marketPlaceRateAndReview = "https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage&ssr=false#review-details";
        private readonly IProcess process;

        public FCCResourcesNavigatorHostObject(IProcess process)
        {
            this.process = process;
        }

#pragma warning disable IDE1006 // Naming Styles
        public void readReadMe()
        {
            process.Start(FCCGithub.Readme);
        }

        public void buyMeACoffee()
        {
            process.Start(payPal);
        }

        public void logIssueOrSuggestion()
        {
            process.Start(FCCGithub.Issues);
        }

        public void rateAndReview()
        {
            process.Start(marketPlaceRateAndReview);
        }
#pragma warning restore IDE1006 // Naming Styles
    }

}
