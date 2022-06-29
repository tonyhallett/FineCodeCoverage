namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using NUnit.Framework;

    public abstract class Readied_TestsBase : TestsBase
    {
        [SetUp]
        public void Ready() => PostStylingWaitForContent.Do(this.EdgeDriver);

    }
}
