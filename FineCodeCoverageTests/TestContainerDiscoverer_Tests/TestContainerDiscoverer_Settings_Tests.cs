using FineCodeCoverage.Options;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    internal class TestContainerDiscoverer_Settings_Tests : TestContainerDiscoverer_Tests_Base
    {
        [Test]
        public void Should_Get_Settings_From_AppOptionsProvider()
        {
            Assert.AreSame(appOptions, testContainerDiscoverer.Settings);
        }

        [Test]
        public void Should_Update_Settings_When_AppOptions_Change()
        {
            var newAppOptions = new Mock<IAppOptions>().Object;
            mockAppOptionsProvider.Raise(appOptionProvider => appOptionProvider.OptionsChanged += null, newAppOptions);

            Assert.AreSame(newAppOptions, testContainerDiscoverer.Settings);
        }
    }
}