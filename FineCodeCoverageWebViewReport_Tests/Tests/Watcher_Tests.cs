namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.IO;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverageWebViewReport;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;

    public class Watcher_Tests : Readied_TestsBase
    {
        private static string tempDirectory;
        private static readonly FileUtil FileUtil = new FileUtil();

        private static string SetupReportPaths()
        {
            tempDirectory = FileUtil.CreateTempDirectory();
            var navigationPath = WriteNavigation(GetHtml("First"));
            var reportPathsPath = WriteSerializedReportPaths(navigationPath);
            return NamedArguments.GetNamedArgument(NamedArguments.ReportPathsPath, reportPathsPath);
        }

        private static string WriteSerializedReportPaths(string navigationPath)
        {
            var reportPaths = new ReportPaths
            {
                ShouldWatch = true,
                NavigationPath = navigationPath
            };
            var reportPathsSerialized = reportPaths.Serialize();
            return Write(reportPathsSerialized, "ReportPaths.json");
        }

        private static string WriteNavigation(string html) => Write(html, "index.html");

        private static string Write(string text, string fileName)
        {
            var path = Path.Combine(tempDirectory, fileName);
            File.WriteAllText(path, text);
            return path;
        }

        private static string GetHtml(string text) => $@"
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

        public Watcher_Tests() : base(SetupReportPaths()) { }

        [SetUp]
        public void NoSetUp() { } // to satisfy NUnit

        [TearDown]
        public void DeleteNavigation()
        {
            if (Directory.Exists(tempDirectory))
            {
                _ = FileUtil.TryDeleteDirectory(tempDirectory);
            }
        }

        private void ChangeHtmlNotifyWatcherAndWait(string text)
        {
            _ = WriteNavigation(GetHtml(text));
            _ = Write("", "watch.txt");

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
