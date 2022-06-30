namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System;
    using System.Diagnostics;
    using OpenQA.Selenium.Appium;
    using OpenQA.Selenium.Appium.Windows;

    public class Appium
    {
        public const string Uri = "http://127.0.0.1:4723/wd/hub";
        private const string DefaultX86Path = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";
        public static Process Start() => Process.Start("appium");
        public static void StartWinAppDriver(string path = DefaultX86Path) => Process.Start(path, Uri);
        public static WindowsDriver CreateWindowsDriver(AppiumOptions appiumOptions) =>
            new WindowsDriver(new Uri(Uri), appiumOptions);
        public static WindowsDriver CreateWindowsDriverFromMainWindowTitle(string mainWindowTitle, AppiumOptions appiumOptions = null)
        {
            appiumOptions = appiumOptions ?? new AppiumOptions();
            _ = appiumOptions.AddTopLevelWindowFromMainWindowTitle(mainWindowTitle);
            return CreateWindowsDriver(appiumOptions);
        }
    }
}
