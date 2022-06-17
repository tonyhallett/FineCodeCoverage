namespace FineCodeCoverageWebViewReport_Tests.SeleniumExtensions
{
    using OpenQA.Selenium;

    public static class With
    {
        public static By Attribute(string attributeName, string attributeValue) =>
            By.CssSelector($"*[{attributeName} = '{attributeValue}']");

        public static By Role(string role) => Attribute("role", role);
    }
}
