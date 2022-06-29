namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.IO;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverageWebViewReport;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;

    public class Watcher_Tests : Readied_TestsBase
    {
        private string tempDirectory;
        private readonly FileUtil fileUtil = new FileUtil();

        private string WriteSerializedReportPaths(string navigationPath)
        {
            var reportPaths = new ReportPaths
            {
                ShouldWatch = true,
                NavigationPath = navigationPath
            };
            var reportPathsSerialized = reportPaths.Serialize();
            return this.Write(reportPathsSerialized, "ReportPaths.json");
        }

        private string WriteNavigation(string html) => this.Write(html, "index.html");

        private string Write(string text, string fileName)
        {
            var path = Path.Combine(this.tempDirectory, fileName);
            File.WriteAllText(path, text);
            return path;
        }

        private string GetHtml(string text) => $@"
<!DOCTYPE html>
<html lang='en'>
    <head><meta charset='utf-8'></head>
    <body>
        <div id='root'>
            <div>{text}</div>
        </div>
    </body>
<html>
";


        [SetUp]
        public void NoSetUp() { } // to satisfy NUnit

        protected override string[] GetFineCodeCoverageWebViewReportArguments()
        {
            this.tempDirectory = this.fileUtil.CreateTempDirectory();
            var navigationPath = this.WriteNavigation(this.GetHtml("First"));
            var reportPathsPath = this.WriteSerializedReportPaths(navigationPath);
            var argument = NamedArguments.GetNamedArgument(NamedArguments.ReportPathsPath, reportPathsPath);
            return new string[] { argument };
        }

        [TearDown]
        public void DeleteNavigation()
        {
            if (Directory.Exists(this.tempDirectory))
            {
                _ = this.fileUtil.TryDeleteDirectory(this.tempDirectory);
            }
        }

        private void ChangeHtmlNotifyWatcherAndWait(string text)
        {
            _ = this.WriteNavigation(this.GetHtml(text));
            _ = this.Write("", "watch.txt");

            _ = this.EdgeDriver.WaitUntil(() => this.EdgeDriver.FindElementByText(text));
        }

        [Test]
        public void Should_Reload_With_Created_And_Changed_Html()
        {
            _ = this.EdgeDriver.FindElementByText("First");

            this.ChangeHtmlNotifyWatcherAndWait("Second");

            this.ChangeHtmlNotifyWatcherAndWait("Third");
        }
    }


}
