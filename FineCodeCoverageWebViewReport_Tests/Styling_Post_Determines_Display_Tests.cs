using NUnit.Framework;
using OpenQA.Selenium;

namespace FineCodeCoverageWebViewReport_Tests
{
    public class Styling_Post_Determines_Display_Tests : TestsBase
    {
        [Test]
        public void Should_Not_Display_If_No_Styling_Received()
        {
            Assert.That(edgeDriver.WaitForContent, Throws.InstanceOf<WebDriverTimeoutException>());
        }

        [Test]
        public void Should_Display_When_Receive_Styling()
        {
            PostStylingWaitForContent.Do(edgeDriver);
        }
    }


}