namespace FineCodeCoverageWebViewReport_Tests.SeleniumExtensions
{
    using OpenQA.Selenium;

    public static class IWebElementExtensions
    {
        public static string GetBackgroundColor(this IWebElement webElement) =>
            webElement.GetCssValue("background-color");

        public static string GetForegroundColor(this IWebElement webElement) =>
            webElement.GetCssValue("foreground-color");

        public static string GetFontFamily(this IWebElement webElement) => webElement.GetCssValue("font-family");

        public static string GetFontSize(this IWebElement webElement) => webElement.GetCssValue("font-size");

        public static string GetAriaHidden(this IWebElement webElement) => webElement.GetAttribute("aria-hidden");

        public static string GetAriaLabel(this IWebElement webElement) => webElement.GetAttribute("aria-label");

        public static string GetInnerHtml(this IWebElement webElement) => webElement.GetAttribute("innerHTML");

        public static string GetOuterHtml(this IWebElement webElement) => webElement.GetAttribute("outerHTML");
        public static string GetInnerText(this IWebElement webElement) => webElement.GetAttribute("innerText");
    }
}
