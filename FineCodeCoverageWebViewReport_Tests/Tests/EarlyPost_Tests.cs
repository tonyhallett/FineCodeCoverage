namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverageWebViewReport;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using Newtonsoft.Json;
    using NUnit.Framework;

    public class EarlyPost_Tests : Readied_TestsBase
    {
        private string tempDirectory;
        private readonly FileUtil fileUtil = new FileUtil();

        protected override string[] GetFineCodeCoverageWebViewReportArguments()
        {
            this.tempDirectory = this.fileUtil.CreateTempDirectory();
            var earlyPosts = new List<ToEarlyPost>
            {
                new ToEarlyPost
                {
                    //KeepAll
                    Type = LogMessageJsonPoster.PostType,
                    PostObject = LogMessage.Simple(MessageContext.Info, "First")
                },
                new ToEarlyPost
                {
                    // KeepLast
                    Type = ReportJsonPoster.PostType,
                    PostObject = PostObjects.CreateReport("Class1")
                },
                new ToEarlyPost
                {
                    // KeepLast
                    Type = ReportJsonPoster.PostType,
                    PostObject = PostObjects.CreateReport("ExistsInLastReport")
                },
                new ToEarlyPost
                {
                    //KeepAll
                    Type = LogMessageJsonPoster.PostType,
                    PostObject = LogMessage.Simple(MessageContext.Info, "Second")
                },
                // todo add styling that will be surpassed by base when have can assert styling
            };
            var serialized = JsonConvert.SerializeObject(earlyPosts);
            var serializedPath = Path.Combine(tempDirectory, "earlyPosts.json");
            fileUtil.WriteAllText(serializedPath, serialized);
            var argument = NamedArguments.GetNamedArgument(NamedArguments.EarlyPostsPath, serializedPath);
            return new string[] { argument };
        }



        [SetUp]
        public void NoSetUp() { } // to satisfy NUnit

        [Test]
        public void Should_Resend_Early_Posts_As_Determined_By_NotReadyPostBehaviour()
        {
            var coverageTabPanel = this.FindCoverageTabPanel();
            this.EdgeDriver.WaitUntil(() => coverageTabPanel.FindElementByAriaLabel("ExistsInLastReport coverage"));

        }

        [TearDown]
        public void DeleteNavigation()
        {
            if (Directory.Exists(this.tempDirectory))
            {
                _ = this.fileUtil.TryDeleteDirectory(this.tempDirectory);
            }
        }

    }
}
