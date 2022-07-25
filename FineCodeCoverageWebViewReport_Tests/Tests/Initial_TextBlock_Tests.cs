namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Diagnostics;
    using NUnit.Framework;
    using OpenQA.Selenium.Appium.Windows;
    using OpenQA.Selenium.Appium;
    using FineCodeCoverageWebViewReport;
    using FineCodeCoverage.Output;
    using OpenQA.Selenium.Support.UI;
    using System;
    using System.IO;
    using OpenQA.Selenium;
    using FineCodeCoverageWebViewReport_Tests.AppiumHelpers;
    using FineCodeCoverage.Automation;

    public class Initial_TextBlock_Tests
    {
        private Process appiumProcess;
        private WindowsDriver windowsDriver;
        private string installWatchedPath;
        private string changeResourcesWatchedFile;
        private string changeResourcesWatchedPath;
        private string installWatchedFile;

        [SetUp]
        public void Setup()
        {
            this.installWatchedFile = Path.GetRandomFileName();
            this.installWatchedPath = Path.Combine(Path.GetTempPath(), this.installWatchedFile);
            this.changeResourcesWatchedFile = Path.GetRandomFileName();
            this.changeResourcesWatchedPath = Path.Combine(Path.GetTempPath(), this.changeResourcesWatchedFile);
            Appium.StartWinAppDriver();
            this.appiumProcess = Appium.Start();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(this.installWatchedPath))
            {
                File.Delete(this.installWatchedPath);
            }
            if (File.Exists(this.changeResourcesWatchedPath))
            {
                File.Delete(this.changeResourcesWatchedPath);
            }
            if (!this.appiumProcess.HasExited)
            {
                _ = this.appiumProcess.CloseMainWindow();
            }
            this.windowsDriver.Quit();
        }

        private AppiumElement FindInitializeTextBlock() =>
            this.windowsDriver.FindElement(
                MobileBy.AccessibilityId(OutputToolWindowControl.InitializeTextBlockAutomationId)
            );

        private AppiumElement AssertInitializeTextBlockDisplayedWith(string expectedText)
        {
            var initializeTextBlock = this.FindInitializeTextBlock();
            Assert.Multiple(() =>
            {
                Assert.That(initializeTextBlock.Displayed, Is.True);
                Assert.That(initializeTextBlock.Text, Is.EqualTo(expectedText));
            });
            return initializeTextBlock;
        }

        private void CreateWindowsDriver(bool initializing)
        {
            var appiumOptions = new AppiumOptions
            {
                App = WebViewReportExeFinder.Find(),
            };
            if (initializing)
            {
                var argument1 = NamedArguments.GetNamedArgument(
                    WebViewRuntimeControlledInstalling.InstalledWatcherFile, this.installWatchedFile
                );
                var argument2 = NamedArguments.GetNamedArgument(
                    ChangingVsResourcesDictionary.ChangeResourcesFileName, this.changeResourcesWatchedFile
                );
                var arguments = $"{argument1} {argument2}";
                _ = appiumOptions.AddAppArguments(arguments);
            }
            this.windowsDriver = new WindowsDriver(appiumOptions);
        }

        [Test]
        public void Should_Show_Loading_Replacing_With_WebView_When_DomContentLoaded()
        {
            this.CreateWindowsDriver(false);

            var initializeTextBlock = this.AssertInitializeTextBlockDisplayedWith("Loading.");

            var webView = new DefaultWait<WindowsDriver>(this.windowsDriver)
                .Until(wd => wd.FindElement(this.ByWebView()));
            Assert.That(webView.Displayed, Is.False);

            _ = new DefaultWait<WindowsDriver>(this.windowsDriver) { Timeout = TimeSpan.FromSeconds(3) }
                .Until(wd => initializeTextBlock.Displayed == false || webView.Displayed);

            Assert.That(initializeTextBlock.Displayed, Is.False);
            Assert.That(webView.Displayed, Is.True);
        }

        [Test]
        public void Should_Show_Installing_Web_View_Runtime_When_Not_Installed()
        {
            this.CreateWindowsDriver(true);

            _ = this.AssertInitializeTextBlockDisplayedWith("Installing Web View Runtime.");

        }

        [Test]
        public void Should_Dynamically_Style_The_Initialize_TextBox()
        {
            this.CreateWindowsDriver(true);

            var initializeTextBlock = this.FindInitializeTextBlock();

            void AssertTextBlockPropertiesFromResources(
                bool initial
            )
            {
                var itemStatus = initializeTextBlock.GetAttribute("ItemStatus");
                var values = DependencyPropertiesItemStatusConverter.ToLookupByDependencyPropertyName(itemStatus);
                Assert.Multiple(() =>
                {
                    Assert.That(values["FontFamily"],
                        Is.EqualTo(
                        initial ? VsResourceValues.VsFontEnvironmentFontFamily :
                        VsResourceValues.ChangedVsFontEnvironmentFontFamily)
                    );
                    Assert.That(values["FontSize"],
                        Is.EqualTo(initial ? VsResourceValues.VsFontEnvironmentFontSize.ToString() :
                        VsResourceValues.ChangedVsFontEnvironmentFontSize.ToString())
                    );
                    Assert.That(values["Background"], Is.EqualTo(initial ? VsResourceValues.VsBrushToolWindowBackgroundColorHex
                        : VsResourceValues.ChangedVsBrushToolWindowBackgroundColorHex)
                    );
                    Assert.That(values["Foreground"], Is.EqualTo(initial ? VsResourceValues.VsBrushToolWindowTextColorHex
                        : VsResourceValues.ChangedVsBrushToolWindowTextColorHex));
                });
            }

            AssertTextBlockPropertiesFromResources(true);

            File.WriteAllText(this.changeResourcesWatchedPath, "this is watched to change resources");

            AssertTextBlockPropertiesFromResources(false);
        }

        private By ByWebView() => MobileBy.AccessibilityId(OutputToolWindowControl.WebViewAutomationId);

        private void Install() => File.WriteAllText(this.installWatchedPath, "this is watched and sets installed");

        [Test]
        public void Should_Show_Loading_This_Takes_Some_Time_When_Becomes_Installed()
        {
            this.CreateWindowsDriver(true);

            Assert.That(this.windowsDriver.FindElements(this.ByWebView()), Is.Empty);

            this.Install();

            var textBlock = this.AssertInitializeTextBlockDisplayedWith("Loading. This takes some time.");
            var webView = new DefaultWait<WindowsDriver>(this.windowsDriver) { Timeout = TimeSpan.FromSeconds(10) }
            .Until(wd =>
             {
                 var wv = wd.FindElement(this.ByWebView());
                 return wv != null && wv.Displayed;
             });
            Assert.That(textBlock.Displayed, Is.False);
        }
    }
}
