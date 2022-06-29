namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverageWebViewReport;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using OpenQA.Selenium;

    public class EarlyPost_Tests : Readied_TestsBase
    {
        private string tempDirectory;
        private readonly FileUtil fileUtil = new FileUtil();

        protected void SetFineCodeCoverageWebViewReportArguments()
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
            var serializedPath = Path.Combine(this.tempDirectory, "earlyPosts.json");
            this.fileUtil.WriteAllText(serializedPath, serialized);
            var argument = NamedArguments.GetNamedArgument(NamedArguments.EarlyPostsPath, serializedPath);
            this.FineCodeCoverageWebViewReportArguments = new string[] { argument };
        }

        public override void Setup()
        {
            this.SetFineCodeCoverageWebViewReportArguments();
            base.Setup();
        }

        [Test]
        public void Should_Resend_Early_Posts_As_Determined_By_NotReadyPostBehaviour()
        {
            var coverageTabPanel = this.FindCoverageTabPanel();
            this.EdgeDriver.WaitUntil(() => coverageTabPanel.FindElementByAriaLabel("ExistsInLastReport coverage"));

            var logTabPanel = this.FindLogTabPanel();
            var activityTexts = this.EdgeDriver.WaitUntilHasElements(() => logTabPanel.FindElements(By.ClassName("ms-ActivityItem-activityText")));
            Assert.That(activityTexts, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(activityTexts[0].GetInnerText(), Is.EqualTo("Second"));
                Assert.That(activityTexts[1].GetInnerText(), Is.EqualTo("First"));
            });
        }

        public override void TearDown()
        {
            if (Directory.Exists(this.tempDirectory))
            {
                _ = this.fileUtil.TryDeleteDirectory(this.tempDirectory);
            }
            base.TearDown();
        }
    }
}
