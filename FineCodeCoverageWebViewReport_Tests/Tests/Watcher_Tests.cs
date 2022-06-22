namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverageWebViewReport;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;

    [TestFixtureSource(nameof(FixtureArgs))]
    public class Watcher_Tests : Readied_TestsBase
    {
        private static string navigationPath;
        private static string tempDirectory;
        private static readonly FileUtil FileUtil = new FileUtil();
        public static List<TestFixtureData> FixtureArgs()
        {
            tempDirectory = FileUtil.CreateTempDirectory();
            navigationPath = Path.Combine(tempDirectory, "index.html");
            File.WriteAllText(navigationPath, FirstHtml);
            var reportPaths = new ReportPaths
            {
                ShouldWatch = true,
                NavigationPath = navigationPath
            };
            var reportPathsSerialized = reportPaths.Serialize();
            var serializedPath = Path.Combine(tempDirectory, "ReportPaths.json");
            File.WriteAllText(serializedPath, reportPathsSerialized);
            return new List<TestFixtureData> { new TestFixtureData(new object[] { serializedPath }) { TestName = "Watcher!" } };
        }

        private static readonly string FirstHtml = @"
<!DOCTYPE html>
<html lang='en'>
    <head><meta charset='utf-8'></head>
    <body>
        <div id='root'>
            <div>First</div>
        </div>
    </body>
<html>
";
        // todo use js to capture the repost
        private readonly string secondHtml = @"
<!DOCTYPE html>
<html lang='en'>
    <head><meta charset='utf-8'></head>
    <body>
        <div id='root'>
            <div>Second</div>
        </div>
    </body>
<html>
";
        public Watcher_Tests(string serializedReportPaths) : base(serializedReportPaths) { }

        [SetUp]
        public void SetUp() { } // to satisfy NUnit

        private void WriteNavigation(string html) => this.Write(html, "index.html");

        private string Write(string text, string fileName)
        {
            var path = Path.Combine(tempDirectory, fileName);
            File.WriteAllText(path, text);
            return path;
        }

        [TearDown]
        public void DeleteNavigation()
        {
            if (Directory.Exists(tempDirectory))
            {
                FileUtil.TryDeleteDirectory(tempDirectory);
            }
        }

        [Test]
        public void Should_Reload_With_Changed_Html()
        {
            _ = this.EdgeDriver.FindElementByText("First");

            this.WriteNavigation(this.secondHtml);
            _ = this.Write("", "watch.txt");

            _ = this.EdgeDriver.WaitUntil(() => this.EdgeDriver.FindElementByText("Second"));
        }
    }


}
