namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using FineCodeCoverageWebViewReport_Tests.EdgeHelpers;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Edge;
    using System.Threading;

    public abstract class EdgeDriverTestsBase
    {
        protected EdgeDriver EdgeDriver { get; private set; }

        protected string[] FineCodeCoverageWebViewReportArguments { set; private get; }

        [SetUp]
        public virtual void Setup()
        {
            var edgeOptions = WebViewEdgeOptions.CreateStart(
                WebViewReportExeFinder.Find(),
                this.FineCodeCoverageWebViewReportArguments);

            this.EdgeDriver = new EdgeDriver(edgeOptions);

            Thread.Sleep(3000);
        }

        protected IWebElement FindTabPanel(string tabPanelToReturn)
        {
            this.EdgeDriver.SelectTab(tabPanelToReturn);

            return this.EdgeDriver.FindNonHiddenTabpanel();
        }

        protected IWebElement FindCoverageTabPanel() => this.FindTabPanel("Coverage");
        protected IWebElement FindLogTabPanel() => this.FindTabPanel("Log");

        [TearDown]
        public virtual void TearDown() => this.EdgeDriver.Quit();

    }
}
