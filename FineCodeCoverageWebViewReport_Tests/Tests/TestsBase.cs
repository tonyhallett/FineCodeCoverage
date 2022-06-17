namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using NUnit.Framework;
    using OpenQA.Selenium.Edge;
    using System.Threading;

    public abstract class TestsBase
    {
        protected EdgeDriver EdgeDriver { get; private set; }

        // if not getting expected results set to true, debug FineCodeCoverageWebViewReport in another vs instance then start the test
        private readonly bool attach = false;

        [SetUp]
        public void Setup()
        {
            var edgeOptions = new EdgeOptions
            {
                UseWebView = true,
                BinaryLocation = @"C:\Users\tonyh\source\repos\FineCodeCoverage\FineCodeCoverageWebViewReport\bin\Debug\FineCodeCoverageWebViewReport.exe",
            };

            if (this.attach)
            {
                edgeOptions.DebuggerAddress = "localhost:9222";
            }

            this.EdgeDriver = new EdgeDriver(edgeOptions);

            Thread.Sleep(3000);

            this.FurtherSetup();
        }

        protected virtual void FurtherSetup() { }

        [TearDown]
        public void TearDown() => this.EdgeDriver.Quit();

    }
}
