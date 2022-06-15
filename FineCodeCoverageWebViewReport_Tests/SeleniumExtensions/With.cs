using OpenQA.Selenium;

namespace FineCodeCoverageWebViewReport_Tests
{
    public static class With
    {
        public static By Attribute(string attributeName, string attributeValue)
        {
            return By.CssSelector($"*[{attributeName} = '{attributeValue}']");
        }

        public static By Role(string role)
        {
            return With.Attribute("role", role);
        }
    }
}