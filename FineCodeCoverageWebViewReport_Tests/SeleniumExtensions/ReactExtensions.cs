using OpenQA.Selenium;

namespace FineCodeCoverageWebViewReport_Tests
{
    public static class ReactExtensions
    {
        public static IWebElement GetRoot(this IWebDriver webDriver)
        {
            return webDriver.FindElement(By.Id("root"));
        }

        // useful for debugging
        public static string GetRootInnerHtml(this IWebDriver webDriver)
        {
            return webDriver.GetRoot().GetInnerHtml();
        }

        public static IWebElement WaitForContent(this IWebDriver webDriver)
        {
            return webDriver.WaitUntil(() => webDriver.GetRoot().FindElement(By.CssSelector("*")), 15);
        }
    }


}