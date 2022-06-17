namespace FineCodeCoverageWebViewReport_Tests.SeleniumExtensions
{
    using OpenQA.Selenium;
    using System.Collections.ObjectModel;
    using System.Linq;

    #region selecting elements notes
    /*
        TagName,
        Id
        ClassName,
        CssSelector - Name is also string text = "*[name =\"" + EscapeCssSelector(nameToFind) + "\"]";
        XPath

        should look for extensions so can follow the react testing framework method of roles
    */
    #endregion

    public static class ISearchContextExtensions
    {
        public static IWebElement FindElementByText(
            this ISearchContext searchContext,
            string text,
            string elementType = "*"
        ) => searchContext.FindElement(By.XPath($"//{elementType}[text()='{text}']"));

        public static IWebElement FindElementByInnerText(
            this ISearchContext searchContext,
            string text,
            string elementType = "*"
        ) => searchContext.FindElement(By.XPath($"//{elementType}[.='{text}']"));

        public static IWebElement FindElementByContainingText(
            this ISearchContext searchContext,
            string text,
            string elementType = "*"
        ) => searchContext.FindElement(By.XPath($"//{elementType}[contains(text(),'{text}')]"));

        public static IWebElement FindButtonByText(
            this ISearchContext searchContext,
            string text
        ) => searchContext.FindElementByText(text, "button");

        public static IWebElement FindButtonByInnerText(
            this ISearchContext searchContext,
            string text
        ) => searchContext.FindElementByInnerText(text, "button");

        public static IWebElement FindElementByRole(this ISearchContext searchContext, string role) =>
            searchContext.FindElementByAttribute("role", role);

        public static IWebElement FindTab(this ISearchContext searchContext, string tabName)
        {
            var tabList = searchContext.FindElementByRole("tablist");
            return tabList.FindElement(By.Name(tabName));
        }

        public static IWebElement FindButton(this ISearchContext searchContext) =>
            searchContext.FindElement(By.TagName("button"));


        public static ReadOnlyCollection<IWebElement> FindButtons(this ISearchContext searchContext) =>
            searchContext.FindElements(By.TagName("button"));

        public static IWebElement FindElementByAriaLabel(this ISearchContext searchContext, string ariaLabel) =>
            searchContext.FindElementByAttribute("aria-label", ariaLabel);

        public static IWebElement FindElementByAttribute(
            this ISearchContext searchContext,
            string attributeName,
            string attributeValue
        ) => searchContext.FindElement(With.Attribute(attributeName, attributeValue));

        public static ReadOnlyCollection<IWebElement> FindElementsByAttribute(
            this ISearchContext searchContext,
            string attributeName,
            string attributeValue
        ) => searchContext.FindElements(With.Attribute(attributeName, attributeValue));

        public static ReadOnlyCollection<IWebElement> FindElementsByRole(
            this ISearchContext searchContext,
            string role
        ) => searchContext.FindElements(With.Attribute("role", role));

        public static ReadOnlyCollection<IWebElement> FindElementsByAriaLabel(
            this ISearchContext searchContext,
            string ariaLabel
        ) => searchContext.FindElementsByAttribute("aria-label", ariaLabel);

        public static IWebElement FindNonHiddenTabpanel(this ISearchContext searchContext) =>
            searchContext.FindElements(With.Role("tabpanel")).First(tabPanel => tabPanel.GetAriaHidden() == "false");
    }
}
