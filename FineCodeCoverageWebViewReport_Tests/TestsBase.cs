using NUnit.Framework;
using OpenQA.Selenium.Edge;
using System.Threading;

namespace FineCodeCoverageWebViewReport_Tests
{
    public abstract class TestsBase
    {
        protected EdgeDriver edgeDriver;

        // if not getting expected results set to true, debug FineCodeCoverageWebViewReport in another vs instance then start the test
        private readonly bool attach;

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

            FurtherSetup();
        }

        protected virtual void FurtherSetup() { }

        [TearDown]
        public void TearDown()
        {
            edgeDriver.Quit();
        }

    }


}