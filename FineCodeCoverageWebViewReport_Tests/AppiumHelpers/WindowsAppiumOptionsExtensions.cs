namespace FineCodeCoverageWebViewReport_Tests.AppiumHelpers
{
    using OpenQA.Selenium.Appium;

    internal static class WindowsAppiumOptionsExtensions
    {
        public static AppiumOptions AddAppArguments(this AppiumOptions appiumOptions, string arguments)
        {
            appiumOptions.AddAdditionalAppiumOption("appArguments", arguments);
            return appiumOptions;
        }
    }
}
