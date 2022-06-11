namespace FineCodeCoverageTests.Coverlet_Tests
{
    using FineCodeCoverage.Core.Coverlet;
    using FineCodeCoverage.Engine.Coverlet;
    using NUnit.Framework;

    public class RunSettingsCoverletConfigurationFactory_Tests
    {
        [Test]
        public void Should_Create_An_Instance_Of_RunSettingsCoverletConfiguration()
        {
            var runSettingsCoverletConfigurationFactory = new RunSettingsCoverletConfigurationFactory();
            Assert.That(runSettingsCoverletConfigurationFactory.Create(), Is.InstanceOf<RunSettingsCoverletConfiguration>());
        }
    }
}
