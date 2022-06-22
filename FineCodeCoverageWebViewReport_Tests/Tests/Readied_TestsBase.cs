namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using NUnit.Framework;

    public abstract class Readied_TestsBase : TestsBase
    {
        public Readied_TestsBase(params string[] fineCodeCoverageWebViewReportArguments) : base(fineCodeCoverageWebViewReportArguments) { }

        [SetUp]
        public void Ready() => PostStylingWaitForContent.Do(this.EdgeDriver);

    }
}
