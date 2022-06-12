namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    using FineCodeCoverage.Engine;
    using NUnit.Framework;

    internal class CoverageToolOutput_Exports_Tests
    {
        [Test]
        [Ignore("FileLoadException Microsoft.VisualStudio.Threading")]
        public void ICoverageToolOutputFolderProvider_Should_Have_Consistent_Ordered_Exports() =>
            MefOrderAssertions.InterfaceExportsHaveConsistentOrder(typeof(ICoverageToolOutputFolderProvider));
    }
}
