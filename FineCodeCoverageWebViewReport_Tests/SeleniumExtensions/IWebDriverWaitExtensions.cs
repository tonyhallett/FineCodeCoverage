namespace FineCodeCoverageWebViewReport_Tests.SeleniumExtensions
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using System;
    using System.Collections.ObjectModel;

    // Note that there is https://github.com/DotNetSeleniumTools/DotNetSeleniumExtras/blob/master/src/WaitHelpers/ExpectedConditions.cs

    public static class IWebDriverWaitExtensions
    {
        public const int DefaultWaitSeconds = 5;
        public static T WaitUntil<T>(this IWebDriver webDriver, Func<T> condition, int seconds = DefaultWaitSeconds) =>
            new WebDriverWait(webDriver, TimeSpan.FromSeconds(seconds))
                .Until(_ => condition());

        public static ReadOnlyCollection<IWebElement> WaitUntilElementsCondition(
            this IWebDriver webDriver,
            Func<ReadOnlyCollection<IWebElement>> getElements,
            Func<ReadOnlyCollection<IWebElement>, bool> condition,
            int seconds = DefaultWaitSeconds
        ) => new WebDriverWait(webDriver, TimeSpan.FromSeconds(seconds))
            .Until(driver =>
            {
                var elements = getElements();
                if (condition(elements))
                {
                    return elements;
                }
                return null;
            });

        public static ReadOnlyCollection<IWebElement> WaitUntilHasElements(
            this IWebDriver webDriver,
            Func<ReadOnlyCollection<IWebElement>> condition,
            int seconds = DefaultWaitSeconds
        ) => webDriver.WaitUntilElementsCondition(condition, elements => elements.Count > 0, seconds);

        public static ReadOnlyCollection<IWebElement> WaitUntilHasNElements(
            this IWebDriver webDriver,
            Func<ReadOnlyCollection<IWebElement>> condition,
            int numElements,
            int seconds = DefaultWaitSeconds
        ) => webDriver.WaitUntilElementsCondition(condition, elements => elements.Count == numElements, seconds);

        public static ReadOnlyCollection<IWebElement> WaitUntilHasAtLeastNElements(
            this IWebDriver webDriver,
            Func<ReadOnlyCollection<IWebElement>> condition,
            int numElements,
            int seconds = DefaultWaitSeconds
        ) => webDriver.WaitUntilElementsCondition(condition, elements => elements.Count >= numElements, seconds);

    }
}
