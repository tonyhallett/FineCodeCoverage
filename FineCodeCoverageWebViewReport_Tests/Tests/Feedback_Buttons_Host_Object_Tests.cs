namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using System.Linq;

    public class Feedback_Buttons_Host_Object_Tests : Readied_TestsBase
    {
        [TestCase("Review", nameof(IFCCResourcesNavigatorHostObject.rateAndReview))]
        [TestCase("Buy me a beer", nameof(IFCCResourcesNavigatorHostObject.buyMeACoffee))]
        [TestCase("Log issue or suggestion", nameof(IFCCResourcesNavigatorHostObject.logIssueOrSuggestion))]
        public void Feedback_Buttons_Should_Invoke_The_FCCResourcesNavigatorHostObject(string buttonAriaLabel, string expectedInvocationName)
        {
            this.EdgeDriver.SelectTab("Feedback");

            var feedbackTabPanel = this.EdgeDriver.FindNonHiddenTabpanel();
            var feedbackButtons = feedbackTabPanel.FindElements(By.TagName("button"));
            var reviewButton = feedbackButtons.First(feedbackButton => feedbackButton.GetAriaLabel() == buttonAriaLabel);
            reviewButton.Click();

            var invocation = this.EdgeDriver.GetFCCResourcesNavigatorHostObjectInvocation();

            Assert.That(invocation.Name, Is.EqualTo(expectedInvocationName));
        }
    }


}
