using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;

namespace FineCodeCoverageWebViewReport_Tests
{
    public class Tests
    {
        private EdgeDriver? edgeDriver;

        [SetUp]
        public void Setup()
        {
            // I can supply arguments.........
            edgeDriver = new EdgeDriver(
                new EdgeOptions
                {
                    UseWebView = true,
                    BinaryLocation = @"C:\Users\tonyh\source\repos\FineCodeCoverage\FineCodeCoverageWebViewReport\bin\Debug\FineCodeCoverageWebViewReport.exe",
                }
            );
        }

        [TearDown]
        public void CloseApp()
        {
            edgeDriver!.Quit();
        }

        [Test]
        public void Test1()
        {
            var wait = new WebDriverWait(edgeDriver, TimeSpan.FromMinutes(1));
            // TagName,
            // Id
            // ClassName,
            // CssSelector - Name is also string text = "*[name =\"" + EscapeCssSelector(nameToFind) + "\"]";
            // XPath

            // should look for extensions so can follow the react testing framework method of roles
            // https://www.nuget.org/packages/Selenium.Axe/

            var summaryElement = wait.Until(driver => driver.FindElementByText("Summary"));
            // summaryElement.GetDomAttribute / GetAttribute difference
            var tagName = summaryElement.TagName;

            var backgroundColor = summaryElement.GetBackgroundColor();
            var foregroundColor = summaryElement.GetBackgroundColor();
            var fontSize = summaryElement.GetFontSize();
            var fontFamily = summaryElement.GetFontFamily();

            edgeDriver!.GetScreenshot().SaveAsFile(@"C:\Users\tonyh\Downloads\testScreenshot.png");
        }
    }

    public static class ISearchContextExtensions
    {
        public static IWebElement FindElementByText(this ISearchContext searchContext, string text)
        {
            return searchContext.FindElement(By.XPath($"//*[text()='{text}']"));
        }

        public static IWebElement FindElementContainingText(this ISearchContext searchContext, string text)
        {
            return searchContext.FindElement(By.XPath($"//*[contains(text(),'{text}')]"));
        }
    }

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
    }
}