namespace FineCodeCoverageWebViewReport_Tests.SeleniumExtensions
{
    using OpenQA.Selenium;

    public static class ReactExtensions
    {
        public static IWebElement GetRoot(this IWebDriver webDriver) => webDriver.FindElement(By.Id("root"));

        // useful for debugging
        public static string GetRootInnerHtml(this IWebDriver webDriver) => webDriver.GetRoot().GetInnerHtml();

        public static IWebElement WaitForContent(this IWebDriver webDriver, int seconds = 15) =>
            webDriver.WaitUntil(() => webDriver.GetRoot().FindElement(By.CssSelector("*")), seconds);
    }


}
