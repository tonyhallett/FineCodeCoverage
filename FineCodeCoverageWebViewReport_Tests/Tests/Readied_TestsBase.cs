namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    public abstract class Readied_TestsBase : TestsBase
    {
        protected override void FurtherSetup() => PostStylingWaitForContent.Do(this.EdgeDriver);
    }
}
