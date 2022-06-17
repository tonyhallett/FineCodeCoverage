using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Linq;

namespace FineCodeCoverageWebViewReport_Tests
{
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
        public static IWebElement FindElementByText(this ISearchContext searchContext, string text)
        {
            return searchContext.FindElement(By.XPath($"//*[text()='{text}']"));
        }

        public static IWebElement FindElementByContainingText(this ISearchContext searchContext, string text)
        {
            return searchContext.FindElement(By.XPath($"//*[contains(text(),'{text}')]"));
        }

        public static IWebElement FindElementByRole(this ISearchContext searchContext, string role)
        {
            return searchContext.FindElementByAttribute("role", role);
        }

        public static IWebElement FindTab(this ISearchContext searchContext, string tabName)
        {
            var tabList = searchContext.FindElementByRole("tablist");
            return tabList.FindElement(By.Name(tabName));
        }
        

        public static IWebElement FindElementByAriaLabel(this ISearchContext searchContext, string ariaLabel)
        {
            return searchContext.FindElementByAttribute("aria-label", ariaLabel);
        }

        public static IWebElement FindElementByAttribute(this ISearchContext searchContext, string attributeName, string attributeValue)
        {
            return searchContext.FindElement(With.Attribute(attributeName, attributeValue));
        }

        public static ReadOnlyCollection<IWebElement> FindElementsByAttribute(this ISearchContext searchContext, string attributeName, string attributeValue)
        {
            return searchContext.FindElements(With.Attribute(attributeName, attributeValue));
        }

        public static ReadOnlyCollection<IWebElement> FindElementsByRole(this ISearchContext searchContext, string role)
        {
            return searchContext.FindElements(With.Attribute("role", role));
        }

        public static ReadOnlyCollection<IWebElement> FindElementsByAriaLabel(this ISearchContext searchContext, string ariaLabel)
        {
            return searchContext.FindElementsByAttribute("aria-label", ariaLabel);
        }

        public static IWebElement FindNonHiddenTabpanel(this ISearchContext searchContext) { 
            return searchContext.FindElements(With.Role("tabpanel")).First(tabPanel => tabPanel.GetAriaHidden() == "false");
        }
    }
}