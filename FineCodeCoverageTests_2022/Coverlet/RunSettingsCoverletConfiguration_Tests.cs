namespace FineCodeCoverageTests.Coverlet_Tests
{
    using FineCodeCoverage.Core.Coverlet;
    using NUnit.Framework;

    public class RunSettingsCoverletConfiguration_Tests
    {
        [Test]
        public void Extract_Should_Return_False_When_No_Coverlet_DataCollector()
        {
            var runSettingsCoverletConfiguration = new RunSettingsCoverletConfiguration();
            var runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""Not XPlat code coverage"">
                <Configuration>
                    <Format> json,cobertura,lcov,teamcity,opencover </Format>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";
            Assert.That(runSettingsCoverletConfiguration.Read(runSettingsXml), Is.False);
        }

        [Test]
        public void Extract_Should_Return_False_When_No_Coverlet_DataCollector_Configuration()
        {
            var runSettingsCoverletConfiguration = new RunSettingsCoverletConfiguration();
            var runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat code coverage"">
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";

            Assert.That(runSettingsCoverletConfiguration.Read(runSettingsXml), Is.False);
        }

        [Test]
        public void Extract_Should_Return_False_When_No_Coverlet_DataCollector_Configuration_Elements()
        {
            var runSettingsCoverletConfiguration = new RunSettingsCoverletConfiguration();
            var runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat code coverage"">
                <Configuration>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";

            Assert.That(runSettingsCoverletConfiguration.Read(runSettingsXml), Is.False);
        }

        [Test]
        public void Extract_Should_Return_False_When_Unknown_Configuration_Element()
        {
            var runSettingsCoverletConfiguration = new RunSettingsCoverletConfiguration();
            var runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat code coverage"">
                <Configuration>
                    <Unknown/>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";

            Assert.That(runSettingsCoverletConfiguration.Read(runSettingsXml), Is.False);
        }

        [Test]
        public void Extract_Should_Return_True_When_Known_Configuration_Element()
        {
            var runSettingsCoverletConfiguration = new RunSettingsCoverletConfiguration();
            var runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat code coverage"">
                <Configuration>
                    <Format/>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";

            Assert.That(runSettingsCoverletConfiguration.Read(runSettingsXml), Is.True);
        }

        [Test]
        public void Should_Set_Configuration_Properties()
        {
            var runSettingsCoverletConfiguration = new RunSettingsCoverletConfiguration();
            var runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat code coverage"">
                <Configuration>
                    <Format>format</Format>
                    <Exclude>exclude</Exclude>
                    <Include>include</Include>
                    <ExcludeByAttribute>excludebyattribute</ExcludeByAttribute>
                    <ExcludeByFile>excludebyfile</ExcludeByFile>
                    <IncludeDirectory>includedirectory</IncludeDirectory>
                    <SingleHit>singlehit</SingleHit>
                    <UseSourceLink>usesourcelink</UseSourceLink>
                    <IncludeTestAssembly>includetestassembly</IncludeTestAssembly>
                    <SkipAutoProps>skipautoprops</SkipAutoProps>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";

            _ = runSettingsCoverletConfiguration.Read(runSettingsXml);

            Assert.Multiple(() =>
            {
                Assert.That(runSettingsCoverletConfiguration.Format, Is.EqualTo("format"));
                Assert.That(runSettingsCoverletConfiguration.Exclude, Is.EqualTo("exclude"));
                Assert.That(runSettingsCoverletConfiguration.Include, Is.EqualTo("include"));
                Assert.That(runSettingsCoverletConfiguration.ExcludeByAttribute, Is.EqualTo("excludebyattribute"));
                Assert.That(runSettingsCoverletConfiguration.ExcludeByFile, Is.EqualTo("excludebyfile"));
                Assert.That(runSettingsCoverletConfiguration.IncludeDirectory, Is.EqualTo("includedirectory"));
                Assert.That(runSettingsCoverletConfiguration.SingleHit, Is.EqualTo("singlehit"));
                Assert.That(runSettingsCoverletConfiguration.UseSourceLink, Is.EqualTo("usesourcelink"));
                Assert.That(runSettingsCoverletConfiguration.IncludeTestAssembly, Is.EqualTo("includetestassembly"));
                Assert.That(runSettingsCoverletConfiguration.SkipAutoProps, Is.EqualTo("skipautoprops"));
            });
        }

        [Test]
        public void Should_Have_Null_Property_Values_For_Missing_Configuration_Elements()
        {
            var runSettingsCoverletConfiguration = new RunSettingsCoverletConfiguration();
            var runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat code coverage"">
                <Configuration>
                    <Format>format</Format>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";

            _ = runSettingsCoverletConfiguration.Read(runSettingsXml);
            Assert.Multiple(() =>
            {
                Assert.That(runSettingsCoverletConfiguration.Exclude, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.Include, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.ExcludeByAttribute, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.ExcludeByFile, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.IncludeDirectory, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.SingleHit, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.UseSourceLink, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.IncludeTestAssembly, Is.Null);
                Assert.That(runSettingsCoverletConfiguration.SkipAutoProps, Is.Null);

                var runSettingsCoverletConfiguration2 = new RunSettingsCoverletConfiguration();
                var runSettingsXml2 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat code coverage"">
                <Configuration>
                    <Exclude>exclude</Exclude>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>";

                _ = runSettingsCoverletConfiguration2.Read(runSettingsXml2);

                Assert.That(runSettingsCoverletConfiguration2.Format, Is.Null);
            });

        }
    }
}
