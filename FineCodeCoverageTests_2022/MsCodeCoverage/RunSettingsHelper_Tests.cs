namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System.Xml.Linq;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using NUnit.Framework;

    public class RunSettingsHelper_Tests
    {
        [Test]
        public void Should_Match_Ms_Data_Collector_By_FriendlyName()
        {
            var element = XElement.Parse(@"
<DataCollector friendlyName='Code Coverage'/>
");
            Assert.That(RunSettingsHelper.IsMsDataCollector(element), Is.True);
        }

        [Test]
        public void Should_Match_Ms_Data_Collector_By_Uri()
        {
            var element = XElement.Parse(@"
<DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0'/>
");
            Assert.That(RunSettingsHelper.IsMsDataCollector(element), Is.True);
        }

        [Test]
        public void Should_Not_Be_Ms_Data_Collector_When_Different_Data_Collector()
        {
            var element = XElement.Parse(@"
<DataCollector uri='other'/>
");
            Assert.That(RunSettingsHelper.IsMsDataCollector(element), Is.False);

            element = XElement.Parse(@"
<DataCollector friendlyName='Other'/>
");
            Assert.That(RunSettingsHelper.IsMsDataCollector(element), Is.False);
        }

        [Test]
        public void Should_Not_Be_Ms_Data_Collector_When_No_Attributes()
        {
            var element = XElement.Parse(@"
<DataCollector />
");
            Assert.That(RunSettingsHelper.IsMsDataCollector(element), Is.False);
        }
    }
}
