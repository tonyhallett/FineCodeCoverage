using OpenQA.Selenium;

namespace FineCodeCoverageWebViewReport_Tests
{
    public static class IWebElementExtensions
    {
        public static string GetBackgroundColor(this IWebElement webElement)
        {
            return webElement.GetCssValue("background-color");
        }

        public static string GetForegroundColor(this IWebElement webElement)
        {
            return webElement.GetCssValue("foreground-color");
        }

        public static string GetFontFamily(this IWebElement webElement)
        {
            return webElement.GetCssValue("font-family");
        }

        public static string GetFontSize(this IWebElement webElement)
        {
            return webElement.GetCssValue("font-size");
        }

        public static string GetAriaHidden(this IWebElement webElement)
        {
            return webElement.GetAttribute("aria-hidden");
        }

        public static string GetAriaLabel(this IWebElement webElement)
        {
            return webElement.GetAttribute("aria-label");
        }

        public static string GetInnerHtml(this IWebElement webElement)
        {
            return webElement.GetAttribute("innerHTML");
        }
    }
}