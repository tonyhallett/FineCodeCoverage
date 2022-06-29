namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    public abstract class Readied_TestsBase : TestsBase
    {
        public override void Setup()
        {
            base.Setup();
            PostStylingWaitForContent.Do(this.EdgeDriver);
        }
    }
}
