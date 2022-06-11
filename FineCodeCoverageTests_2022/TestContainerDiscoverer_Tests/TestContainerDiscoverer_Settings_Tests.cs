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
            Assert.That(this.TestContainerDiscoverer.Settings, Is.SameAs(this.AppOptions));
        }

        [Test]
        public void Should_Update_Settings_When_AppOptions_Change()
        {
            var newAppOptions = new Mock<IAppOptions>().Object;
            this.MockAppOptionsProvider.Raise(appOptionProvider => appOptionProvider.OptionsChanged += null, newAppOptions);

            Assert.That(this.TestContainerDiscoverer.Settings, Is.SameAs(newAppOptions));
        }
    }
}