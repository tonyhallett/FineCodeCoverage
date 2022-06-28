namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Edge;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public abstract class TestsBase
    {
        protected EdgeDriver EdgeDriver { get; private set; }

        private readonly string[] fineCodeCoverageWebViewReportArguments;

        public TestsBase() { }
        public TestsBase(params string[] fineCodeCoverageWebViewReportArguments) =>
            this.fineCodeCoverageWebViewReportArguments = fineCodeCoverageWebViewReportArguments;

        [SetUp]
        public void Setup()
        {
            var edgeOptions = new EdgeOptions
            {
                UseWebView = true,
                BinaryLocation = this.GetFineCodeCoverageWebViewReportExeLocation(),
            };

            if (this.fineCodeCoverageWebViewReportArguments != null)
            {
                edgeOptions.AddArguments(this.fineCodeCoverageWebViewReportArguments);
            }

            this.EdgeDriver = new EdgeDriver(edgeOptions);

            Thread.Sleep(3000);
        }

        private string GetFineCodeCoverageWebViewReportExeLocation()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var fccSolutionDirectory = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation));
            while (true)
            {
                fccSolutionDirectory = fccSolutionDirectory.Parent;
                if (fccSolutionDirectory.Name == "FineCodeCoverage")
                {
                    break;
                }
            }
            return Path.Combine(fccSolutionDirectory.FullName, "FineCodeCoverageWebViewReport", "bin", "Debug", "FineCodeCoverageWebViewReport.exe");

        }


        protected IWebElement FindTabPanel(string tabPanelToReturn)
        {
            this.EdgeDriver.SelectTab(tabPanelToReturn);

            return this.EdgeDriver.FindNonHiddenTabpanel();
        }

        protected IWebElement FindCoverageTabPanel() => this.FindTabPanel("Coverage");

        [TearDown]
        public void TearDown() => this.EdgeDriver.Quit();

    }
}
