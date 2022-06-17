using OpenQA.Selenium;

namespace FineCodeCoverageWebViewReport_Tests
{
    public static class ClickExtensions {
        public static void SelectTab(this IWebDriver webDriver, string tabName)
        {
            webDriver.FindTab(tabName).Click();
        }
    }
}