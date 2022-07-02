namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using FineCodeCoverageWebViewReport.JsonPosterRegistration;
    using FineCodeCoverageWebViewReport_Tests.EdgeHelpers;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;

    public class Log_Tests : Readied_TestsBase
    {
        private void SendLogMessageAndClickFCCLink(string hostObject, string methodName, string ariaLabel)
        {
            var logMessage = new LogMessage
            {
                context = MessageContext.Info,
                message = new LogMessagePart[]
                {
                    new FCCLink
                    {
                        hostObject = hostObject,
                        methodName = methodName,
                        ariaLabel = ariaLabel
                    }
                }
            };

            var logTabPanel = this.FindLogTabPanel();

            this.EdgeDriver.ExecutePostBack(LogJsonPosterRegistration.RegistrationName, logMessage);

            this.EdgeDriver.WaitUntil(() => logTabPanel.FindElementByAriaLabel(ariaLabel)).Click();
        }

        [Test]
        public void Should_FCCOutputPaneHostObject_show_When_Click_FCCLink_Show_FCC_Output_Pane()
        {
            this.SendLogMessageAndClickFCCLink(
                FCCOutputPaneHostObjectRegistration.HostObjectName,
                nameof(FCCOutputPaneHostObject.show),
                "Show the pane"
            );

            var invocation = this.EdgeDriver.GetFCCOutputPaneHostObjectInvocation();
            Assert.That(invocation.Name, Is.EqualTo(nameof(FCCOutputPaneHostObject.show)));
        }

        [Test]
        public void Should_FCCResourcesNavigatorHostObject_readReadMe_When_Click_FCCLink_View_ReadMe()
        {
            this.SendLogMessageAndClickFCCLink(
                FCCResourcesNavigatorRegistration.HostObjectName,
                nameof(FCCResourcesNavigatorHostObject.readReadMe),
                "view readme"
            );

            var invocation = this.EdgeDriver.GetFCCResourcesNavigatorHostObjectInvocation();
            Assert.That(invocation.Name, Is.EqualTo(nameof(FCCResourcesNavigatorHostObject.readReadMe)));

        }
    }
}
