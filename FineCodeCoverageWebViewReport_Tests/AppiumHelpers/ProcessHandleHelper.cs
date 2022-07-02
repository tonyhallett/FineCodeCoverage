namespace FineCodeCoverageWebViewReport_Tests.AppiumHelpers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using OpenQA.Selenium.Appium;

    public static class ProcessHandleHelper
    {
        public static AppiumOptions AddTopLevelWindow(this AppiumOptions appiumOptions, string topLevelWindowHandle)
        {
            appiumOptions.AddAdditionalAppiumOption("appTopLevelWindow", topLevelWindowHandle);
            return appiumOptions;
        }

        public static AppiumOptions AddTopLevelWindowFromMainWindowTitle(
            this AppiumOptions appiumOptions,
            string mainWindowTitle
        ) => appiumOptions.AddTopLevelWindow(FindProcessHandleByMainWindowTitle(mainWindowTitle));

        public static string FindProcessHandleByMainWindowTitle(string mainWindowTitle) =>
            FindProcessBy(process => process.MainWindowTitle == mainWindowTitle);


        public static string FindProcessBy(Func<Process, bool> predicate)
        {
            var matchingProcess = Process.GetProcesses().First(predicate);
            return matchingProcess.MainWindowHandle.ToString("x");
        }
    }
}
