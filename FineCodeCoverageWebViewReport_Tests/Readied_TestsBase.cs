namespace FineCodeCoverageWebViewReport_Tests
{
    public abstract class Readied_TestsBase : TestsBase
    {
        protected override void FurtherSetup()
        {
            PostStylingWaitForContent.Do(edgeDriver);
        }
    }


}