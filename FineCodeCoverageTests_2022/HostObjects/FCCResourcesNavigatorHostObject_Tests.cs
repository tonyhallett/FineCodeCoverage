namespace FineCodeCoverageTests.HostObjectTests
{
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.HostObjects;
    using Moq;
    using NUnit.Framework;

    internal class FCCResourcesNavigatorHostObject_Tests
    {
        private Mock<IProcess> mockProcess;
        private FCCResourcesNavigatorRegistration fccResourcesNavigatorRegistration;
        private FCCResourcesNavigatorHostObject fccResourcesNavigatorHostObject;

        [SetUp]
        public void SetUp()
        {
            this.mockProcess = new Mock<IProcess>();
            this.fccResourcesNavigatorRegistration = new FCCResourcesNavigatorRegistration(this.mockProcess.Object);
            this.fccResourcesNavigatorHostObject = this.fccResourcesNavigatorRegistration.HostObject as FCCResourcesNavigatorHostObject;
        }
        [Test]
        public void Should_Have_Name() =>
            Assert.That(this.fccResourcesNavigatorRegistration.Name, Is.EqualTo(FCCResourcesNavigatorRegistration.HostObjectName));

        private void VerifyProcess(string processStart) => this.mockProcess.Verify(process => process.Start(processStart));

        [Test]
        public void Should_Open_Browser_At_FCC_Github_ReadMe_When_readReadMe_Called_From_Js()
        {
            this.fccResourcesNavigatorHostObject.readReadMe();
            this.VerifyProcess("https://github.com/FortuneN/FineCodeCoverage/blob/master/README.md");
        }

        [Test]
        public void Should_Open_Browser_At_Fortune_Paypal_When_buyMeACoffee_Called_From_Js()
        {
            this.fccResourcesNavigatorHostObject.buyMeACoffee();
            this.VerifyProcess("https://paypal.me/FortuneNgwenya");
        }

        [Test]
        public void Should_Open_Browser_At_FCC_Github_Issues_When_logIssueOrSuggestion_Called_From_Js()
        {
            this.fccResourcesNavigatorHostObject.logIssueOrSuggestion();
            this.VerifyProcess("https://github.com/FortuneN/FineCodeCoverage/issues");
        }

        [Test]
        public void Should_Open_Browser_At_Vs_Market_Place_When_rateAndReview_Called_From_Js()
        {
            this.fccResourcesNavigatorHostObject.rateAndReview();
            this.VerifyProcess("https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage&ssr=false#review-details");
        }
    }
}
