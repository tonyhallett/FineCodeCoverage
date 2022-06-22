namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using NUnit.Framework;
    using OpenQA.Selenium;
    public class Styling_Post_Determines_Display_Tests : TestsBase
    {
        [Test]
        public void Should_Not_Display_If_No_Styling_Received() =>
            Assert.That(() => this.EdgeDriver.WaitForContent(), Throws.InstanceOf<WebDriverTimeoutException>());

        [Test]
        public void Should_Display_When_Receive_Styling() => PostStylingWaitForContent.Do(this.EdgeDriver);
    }


}
