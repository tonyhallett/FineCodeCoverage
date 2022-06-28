namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
        private static string tempDirectory;
        private static readonly FileUtil FileUtil = new FileUtil();

        public EarlyPost_Tests() : base(PathToSerializedEarlyPosts()) { }

        private static string PathToSerializedEarlyPosts()
        {
            tempDirectory = FileUtil.CreateTempDirectory();
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
                    PostObject = PostObjects.CreateReport("FirstClassRenamed")
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
            FileUtil.WriteAllText(serializedPath, serialized);
            return NamedArguments.GetNamedArgument(NamedArguments.EarlyPostsPath, serializedPath);
        }

        [SetUp]
        public void NoSetUp() { } // to satisfy NUnit

        [Test]
        public void Should_Resend_Early_Posts_As_Determined_By_NotReadyPostBehaviour()
        {
            var coverageTabPanel = this.FindCoverageTabPanel();
            // todo add the name of the class to the aria-label / aria-label the row
            var classOpenerButton = this.FindFirstClassOpener(coverageTabPanel);
            var buttonInnerHtml = classOpenerButton.GetInnerHtml();

            Assert.That(buttonInnerHtml.Contains("Class1"), Is.False);
            Assert.That(buttonInnerHtml.Contains("FirstClassRenamed"), Is.True);

        }


        private IWebElement FindFirstClassOpener(IWebElement coverageTabPanel)
        {
            var firstRowGroup = this.EdgeDriver.WaitUntil(() => coverageTabPanel.FindElementByRole("rowgroup"), 5);
            var row = firstRowGroup.FindElementsByRole("row").ElementAt(0);
            return row.FindElementByAriaLabel("Open in Visual Studio");
        }

        [TearDown]
        public void DeleteNavigation()
        {
            if (Directory.Exists(tempDirectory))
            {
                _ = FileUtil.TryDeleteDirectory(tempDirectory);
            }
        }

    }
}
