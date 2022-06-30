namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Diagnostics;
    using NUnit.Framework;
    using OpenQA.Selenium.Appium.Windows;

    public class ShowsInstallingWpfUIWhenInstalling_Tests : TestsBase
    {
        private Process appiumProcess;
        private WindowsDriver windowsDriver;

        [SetUp]
        public void SetupAppium()
        {
            Appium.StartWinAppDriver();
            this.appiumProcess = Appium.Start();
            this.windowsDriver = Appium.CreateWindowsDriverFromMainWindowTitle("FCCWebViewReport");
        }

        [TearDown]
        public void TearDownAppium() => _ = this.appiumProcess.CloseMainWindow();

    }
}
