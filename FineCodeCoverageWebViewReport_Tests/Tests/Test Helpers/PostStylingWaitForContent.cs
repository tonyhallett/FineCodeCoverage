namespace FineCodeCoverageWebViewReport_Tests
{
    using FineCodeCoverageWebViewReport.JsonPosterRegistration;
    using FineCodeCoverageWebViewReport_Tests.EdgeHelpers;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using OpenQA.Selenium.Edge;

    public static class PostStylingWaitForContent
    {
        public static void Do(EdgeDriver edgeDriver, int seconds = 15)
        {
            edgeDriver.ExecutePostBack(StylingJsonPosterRegistration.RegistrationName, PostObjects.GetStyling());

            _ = edgeDriver.WaitForContent(seconds);
        }
    }
}
