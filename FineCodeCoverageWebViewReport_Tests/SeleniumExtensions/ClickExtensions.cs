namespace FineCodeCoverageWebViewReport_Tests.SeleniumExtensions
{
    using OpenQA.Selenium;

    public static class ClickExtensions
    {
        public static void SelectTab(this IWebDriver webDriver, string tabName) =>
            webDriver.FindTab(tabName).Click();
    }
}
