namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using FineCodeCoverageWebViewReport.InvocationsRecordingRegistration;
    using FineCodeCoverageWebViewReport.JsonPosterRegistration;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using System.Linq;

    public class SourceFileOpener_Tests : Readied_TestsBase
    {
        [SetUp]
        public void PostReport() => this.EdgeDriver.ExecutePostBack(
            ReportJsonPosterRegistration.RegistrationName, PostObjects.Report
        );

        private IWebElement FindClassOpenerButton(IWebElement coverageTabPanel, int rowIndex)
        {
            var firstRowGroup = this.EdgeDriver.WaitUntil(() => coverageTabPanel.FindElementByRole("rowgroup"), 5);
            var row = firstRowGroup.FindElementsByRole("row").ElementAt(rowIndex);
            return row.FindElementByAriaLabel("Open in Visual Studio");
        }

        private IWebElement FindTabPanel(string tabPanelToReturn)
        {
            this.EdgeDriver.SelectTab(tabPanelToReturn);

            return this.EdgeDriver.FindNonHiddenTabpanel();
        }

        private Invocation Click_Summary_Class_Open_Button(int row)
        {
            var coverageTabPanel = this.FindTabPanel("Coverage");

            var classOpenerButton = this.FindClassOpenerButton(coverageTabPanel, row);
            classOpenerButton.Click();

            return this.EdgeDriver.GetSourceFileOpenerHostObjectInvocation();
        }

        [Test]
        public void Should_Invoke_SourceFileOpenerHostObject_openFiles_With_Single_Path_When_Invoke_Summary_Class_Open_File_Button_For_Non_Partial_Class()
        {
            var invocation = this.Click_Summary_Class_Open_Button(0);

            Assert.That(invocation.Name, Is.EqualTo("openFiles"));
            Assert.That(invocation.Arguments, Has.Count.EqualTo(1));
            Assert.That(invocation.Arguments[0], Is.EqualTo("Class1Path"));
        }

        private void AssertOpenMultipleFiles(Invocation invocation)
        {
            Assert.That(invocation.Name, Is.EqualTo(nameof(SourceFileOpenerInvocationsHostObject.openFiles)));
            Assert.That(invocation.Arguments, Has.Count.EqualTo(2));
            Assert.That(invocation.Arguments[0], Is.EqualTo("Class2Path1"));
            Assert.That(invocation.Arguments[1], Is.EqualTo("Class2Path2"));
        }

        [Test]
        public void Should_Invoke_SourceFileOpenerHostObject_openFiles_With_Multiple_Paths_When_Invoke_Summary_Class_Open_File_Button_For_Partial_Class_Across_Multiple_Files()
        {
            var invocation = this.Click_Summary_Class_Open_Button(1);

            this.AssertOpenMultipleFiles(invocation);
        }

        private Invocation Click_Riskhotspot_Button(bool isClass)
        {
            var riskHotspotsTabPanel = this.FindTabPanel("Risk Hotspots");

            var rowElements = this.EdgeDriver.WaitUntilHasElements(
                () => riskHotspotsTabPanel.FindElementsByRole("row"),
                5
            );
            var rowElement = rowElements.ElementAt(1);
            var openButtons = rowElement.FindElementsByAriaLabel("Open in Visual Studio");
            var openButton = openButtons[isClass ? 0 : 1];
            openButton.Click();

            return this.EdgeDriver.GetSourceFileOpenerHostObjectInvocation();
        }

        [Test]
        public void Should_Invoke_SourceFileOpenerHostObject_openFiles_With_Multiple_Paths_When_Invoke_Riskhotspot_Class_Open_File_Button_For_Partial_Class_Across_Multiple_Files()
        {
            var invocation = this.Click_Riskhotspot_Button(true);

            this.AssertOpenMultipleFiles(invocation);
        }

        [Test]
        public void Should_Invoke_SourceFileOpenerHostObject_openAtLine_When_Invoke_Riskhotspot_Class_Open_File_Button_For_Partial_Class_Across_Multiple_Files()
        {
            var invocation = this.Click_Riskhotspot_Button(false);

            Assert.That(invocation.Name, Is.EqualTo(nameof(SourceFileOpenerInvocationsHostObject.openAtLine)));

            Assert.That(invocation.Arguments[0], Is.EqualTo("Class2Path2"));
            Assert.That(invocation.Arguments[1], Is.EqualTo(123));
        }
    }


}
