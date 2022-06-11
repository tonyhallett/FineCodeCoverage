using FineCodeCoverage.Engine;
using NUnit.Framework;

namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    class CoverageToolOutput_Exports_Tests
    {
        [Test]
        [Ignore("FileLoadException Microsoft.VisualStudio.Threading")]
        public void ICoverageToolOutputFolderProvider_Should_Have_Consistent_Ordered_Exports()
        {
            MefOrderAssertions.InterfaceExportsHaveConsistentOrder(typeof(ICoverageToolOutputFolderProvider));
        }
    }
}
