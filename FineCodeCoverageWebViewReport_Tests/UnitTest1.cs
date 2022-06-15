using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Output.JsSerialization.ReportGenerator;
using FineCodeCoverageWebViewReport.InvocationsRecordingRegistration;
using FineCodeCoverageWebViewReport.JsonPosterRegistration;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FineCodeCoverageWebViewReport_Tests
{
    public class Tests
    {
        private EdgeDriver? edgeDriver;

        // if not getting expected results set to true, debug FineCodeCoverageWebViewReport in another vs instance then start the test
        private bool attach;

        [SetUp]
        public void Setup()
        {
            var edgeOptions = new EdgeOptions
            {
                UseWebView = true,
                BinaryLocation = @"C:\Users\tonyh\source\repos\FineCodeCoverage\FineCodeCoverageWebViewReport\bin\Debug\FineCodeCoverageWebViewReport.exe",
            };

            if (attach)
            {
                edgeOptions.DebuggerAddress = "localhost:9222";
            }

            edgeDriver = new EdgeDriver(edgeOptions);

            Thread.Sleep(3000);
        }

        [TearDown]
        public void CloseApp()
        {
            edgeDriver!.Quit();
        }

        private static Styling GetStyling()
        {
            Dictionary<string, Dictionary<string, string>> categoryColours = new()
            {
                {
                    "EnvironmentColors",
                    new Dictionary<string, string>
                    {
                        { "ToolWindowText", "rgb(0,0,0)"},
                        { "ToolWindowBackground", "rgb(0,0,0)"}
                    }
                }
            };

            var styling = new Styling
            {
                fontName = "Arial",
                fontSize = "10px",
                categoryColours = categoryColours
            };

            return styling;
        }

        private void ExecutePostBack(string hostObjectRegistrationName,object objectToPostBack)
        {
            var serialized = JsonConvert.SerializeObject(objectToPostBack);
            edgeDriver!.ExecuteScript(
                $"window.chrome.webview.hostObjects.{hostObjectRegistrationName}.{nameof(IHostObject.postBack)}(arguments[0])",
                serialized
            );
        }

        private List<Invocation> ExecuteGetInvocations(string hostObjectRegistrationName)
        {
            // todo nameof when have refactored to base HostObject
            var serialized = edgeDriver!.ExecuteScript(
                $"return window.chrome.webview.hostObjects.{hostObjectRegistrationName}.getInvocations()"
            );
            var invocations =  JsonConvert.DeserializeObject<List<Invocation>>((string)serialized!);
            return invocations;
        }

        private IWebElement WaitForContent()
        {
            return new WebDriverWait(edgeDriver, TimeSpan.FromSeconds(15))
                .Until(driver =>
                {
                    var root = driver.FindElement(By.Id("root"));
                    return root.FindElement(By.CssSelector("*"));
                });
        }

        [Test]
        public void Should_Not_Display_If_No_Styling_Received()
        {
            Assert.That(WaitForContent, Throws.InstanceOf<WebDriverTimeoutException>());
        }

        private void WaitForStyledContent()
        {
            ExecutePostBack(StylingJsonPosterRegistration.RegistrationName, GetStyling());

            WaitForContent();
        }

        [Test]
        public void Should_Display_When_Receive_Styling()
        {
            WaitForStyledContent();
            
            // could drive the ui and take screen shots for the github repo
            // edgeDriver!.GetScreenshot().SaveAsFile(@"C:\Users\tonyh\Downloads\testScreenshot.png");
        }

        private void SelectTab(string tabName)
        {
            var tabList = edgeDriver!.FindElementByRole("tablist");
            var tab = tabList.FindElement(By.Name(tabName));
            tab.Click();
        }

        [TestCase("Review", nameof(IFCCResourcesNavigatorHostObject.rateAndReview))]
        [TestCase("Buy me a beer", nameof(IFCCResourcesNavigatorHostObject.buyMeACoffee))]
        [TestCase("Log issue or suggestion", nameof(IFCCResourcesNavigatorHostObject.logIssueOrSuggestion))]
        public void Feedback_Buttons_Should_Invoke_The_FCCResourcesNavigatorHostObject(string buttonAriaLabel, string expectedInvocationName)
        {
            WaitForStyledContent();

            SelectTab("Feedback");

            var feedbackTabPanel = edgeDriver!.FindNonHiddenTabpanel();
            var feedbackButtons = feedbackTabPanel.FindElements(By.TagName("button"));
            var reviewButton = feedbackButtons.First(feedbackButton => feedbackButton.GetAriaLabel() == buttonAriaLabel);
            reviewButton.Click();

            var invocations = ExecuteGetInvocations(FCCResourcesNavigatorRegistration.HostObjectName);

            Assert.That(invocations.Count, Is.EqualTo(1));
            Assert.That(invocations[0].Name, Is.EqualTo(expectedInvocationName));
        }

        private void SendReport()
        {
            throw new NotImplementedException();
            // May need to instead refactor to interface .... including the factory and removal of T in Payload<T>
            Report report = null; 
            ExecutePostBack(ReportJsonPosterRegistration.RegistrationName, report);
        }

        private IWebElement FindFirstClassOpenerButton(IWebElement coverageTabPanel)
        {
            throw new NotImplementedException();
        }

        [Test] // there will need to be two tests for this 
        public void Class_File_Buttons_Should_Invoke_The_SourceFileOpenerHostObject()
        {
            throw new NotImplementedException();
            WaitForStyledContent();

            SendReport();
            SelectTab("Coverage");

            var coverageTabPanel = edgeDriver!.FindNonHiddenTabpanel();
            var classOpenerButton = FindFirstClassOpenerButton(coverageTabPanel);
            classOpenerButton.Click();

            var invocations = ExecuteGetInvocations(SourceFileOpenerHostObjectRegistration.HostObjectName);

            Assert.That(invocations.Count, Is.EqualTo(1));
            Assert.That(invocations[0].Name, Is.EqualTo(""));
        }
    }
}