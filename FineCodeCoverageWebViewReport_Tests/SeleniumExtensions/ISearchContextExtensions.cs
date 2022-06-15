using OpenQA.Selenium;
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

        public static IWebElement FindElementContainingText(this ISearchContext searchContext, string text)
        {
            return searchContext.FindElement(By.XPath($"//*[contains(text(),'{text}')]"));
        }

        public static IWebElement FindElementByRole(this ISearchContext searchContext, string role)
        {
            return searchContext.FindElementByAttribute("role", role);
        }

        public static IWebElement FindElementByAttribute(this ISearchContext searchContext, string attributeName, string attributeValue)
        {
            return searchContext.FindElement(With.Attribute(attributeName, attributeValue));
        }

        public static IWebElement FindNonHiddenTabpanel(this ISearchContext searchContext) { 
            return searchContext.FindElements(With.Role("tabpanel")).First(tabPanel => tabPanel.GetAriaHidden() == "false");
        }
    }
}