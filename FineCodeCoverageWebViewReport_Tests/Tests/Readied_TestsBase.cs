namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    public abstract class Readied_TestsBase : EdgeDriverTestsBase
    {
        public override void Setup()
        {
            base.Setup();
            PostStylingWaitForContent.Do(this.EdgeDriver);
        }
    }
}
