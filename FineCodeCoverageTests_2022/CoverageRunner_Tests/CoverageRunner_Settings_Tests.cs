namespace FineCodeCoverageTests.CoverageRunner_Tests
{
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    internal class CoverageRunner_Settings_Tests : CoverageRunner_Tests_Base
    {
        [Test]
        public void Should_Get_Settings_From_AppOptionsProvider() =>
            Assert.That(this.CoverageRunner.Settings, Is.SameAs(this.AppOptions));

        [Test]
        public void Should_Update_Settings_When_AppOptions_Change()
        {
            var newAppOptions = new Mock<IAppOptions>().Object;
            this.MockAppOptionsProvider.Raise(appOptionProvider => appOptionProvider.OptionsChanged += null, newAppOptions);

            Assert.That(this.CoverageRunner.Settings, Is.SameAs(newAppOptions));
        }
    }
}
