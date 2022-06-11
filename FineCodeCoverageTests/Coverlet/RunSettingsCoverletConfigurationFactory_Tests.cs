using FineCodeCoverage.Core.Coverlet;
using FineCodeCoverage.Engine.Coverlet;
using NUnit.Framework;

namespace FineCodeCoverageTests.Coverlet_Tests
{
    public class RunSettingsCoverletConfigurationFactory_Tests
    {
        [Test]
        public void Should_Create_An_Instance_Of_RunSettingsCoverletConfiguration()
        {
            var runSettingsCoverletConfigurationFactory = new RunSettingsCoverletConfigurationFactory();
            Assert.IsInstanceOf<RunSettingsCoverletConfiguration>(runSettingsCoverletConfigurationFactory.Create());
        }
    }
}