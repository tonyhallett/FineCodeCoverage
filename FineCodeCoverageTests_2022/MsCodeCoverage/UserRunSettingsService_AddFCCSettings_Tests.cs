namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System.Collections.Generic;
    using System.Xml.XPath;
    using AutoMoq;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverageTests.MsCodeCoverage_Tests.Helpers;
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using Moq;
    using NUnit.Framework;

    internal class UserRunSettingsService_AddFCCSettings_Tests
    {
        private AutoMoqer autoMocker;
        private UserRunSettingsService userRunSettingsService;
        private const string UnchangedRunConfiguration = @"
<RunConfiguration>
    <TestAdaptersPaths>SomePath</TestAdaptersPaths>
</RunConfiguration>
";
        private const string UnchangedDataCollectionRunSettings = @"
<DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName='Code Coverage'>
                <Configuration>
                    <Format>Cobertura</Format>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
";
        private readonly string noRunConfigurationSettings =
$@"<RunSettings>
    {UnchangedDataCollectionRunSettings}
</RunSettings>
";
        private const string MsDataCollectorIncludeCompanyNamesReplacements = @"
<DataCollector friendlyName='Code Coverage' enabled='true'>
    <Configuration>
        <CodeCoverage>
            <ModulePaths>
                <Exclude></Exclude>
                <Include></Include>
            </ModulePaths>
            <Functions>
                <Exclude></Exclude>
                <Include></Include>
            </Functions>
            <Attributes>
                <Exclude></Exclude>
                <Include></Include>
            </Attributes>
            <Sources>
                <Exclude></Exclude>
                <Include></Include>
            </Sources>
            <CompanyNames>
                <Exclude></Exclude>
                <Include>
                    <CompanyName>Replacement</CompanyName>
                </Include>
            </CompanyNames>
            <PublicKeyTokens>
                <Exclude></Exclude>
                <Include></Include>
            </PublicKeyTokens>
        </CodeCoverage>
        <Format>Cobertura</Format>
        <FCCGenerated/>
    </Configuration>
</DataCollector>
";

        [SetUp]
        public void CreateSut()
        {
            this.autoMocker = new AutoMoqer();
            this.autoMocker.SetInstance<IRunSettingsTemplate>(new RunSettingsTemplate());
            this.userRunSettingsService = this.autoMocker.Create<UserRunSettingsService>();
        }

        [Test]
        public void Should_Not_Process_When_Runsettings_Created_From_Template()
        {
            var xPathNavigable = new Mock<IXPathNavigable>().Object;

            this.autoMocker = new AutoMoqer();
            _ = this.autoMocker.GetMock<IRunSettingsTemplate>()
                .Setup(runSettingsTemplate => runSettingsTemplate.FCCGenerated(xPathNavigable)).Returns(true);

            this.userRunSettingsService = this.autoMocker.Create<UserRunSettingsService>();

            Assert.That(this.userRunSettingsService.AddFCCRunSettings(xPathNavigable, null, null, null), Is.Null);
        }

        [Test]
        public void Should_Create_Replacements()
        {
            var xPathNavigable = XmlHelper.CreateXPathNavigable("<RunSettings/>");

            var mockRunSettingsConfigurationInfo = new Mock<IRunSettingsConfigurationInfo>();
            var testContainers = new List<ITestContainer> { new Mock<ITestContainer>().Object };
            _ = mockRunSettingsConfigurationInfo.SetupGet(
                    runSettingsConfigurationInfo => runSettingsConfigurationInfo.TestContainers
                ).Returns(testContainers);

            var projectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();

            var mockRunSettingsTemplateReplacementsFactory = this.autoMocker.GetMock<IRunSettingsTemplateReplacementsFactory>();
            _ = mockRunSettingsTemplateReplacementsFactory.Setup(
                runSettingsTemplateReplacementsFactory => runSettingsTemplateReplacementsFactory.Create(
                    testContainers,
                    projectDetailsLookup,
                    "fccMsTestAdapterPath"
                )
            ).Returns(new RunSettingsTemplateReplacements());

            _ = this.userRunSettingsService.AddFCCRunSettings(
                xPathNavigable,
                mockRunSettingsConfigurationInfo.Object,
                projectDetailsLookup,
                "fccMsTestAdapterPath"
            );

            mockRunSettingsTemplateReplacementsFactory.VerifyAll();
        }

        [Test]
        public void Should_Add_Replaced_RunConfiguration_If_Not_Present()
        {
            var runSettings = this.noRunConfigurationSettings;

            var resultsDirectory = "Results_Directory";
            var testAdapter = "MsTestAdapterPath";
            var expectedRunSettings = $@"
        <RunSettings>
            <RunConfiguration>
                <ResultsDirectory>{resultsDirectory}</ResultsDirectory>
                <TestAdaptersPaths>{testAdapter}</TestAdaptersPaths>
                <CollectSourceInformation>False</CollectSourceInformation>
            </RunConfiguration>
            {UnchangedDataCollectionRunSettings}
        </RunSettings>
        ";
            this.TestAddFCCSettings(
                runSettings,
                expectedRunSettings,
                new RunSettingsTemplateReplacements
                {
                    ResultsDirectory = resultsDirectory,
                    TestAdapter = testAdapter
                }
            );
        }

        [Test]
        public void Should_Add_Replaced_TestAdaptersPath_If_Not_Present()
        {
            var runSettings = $@"
        <RunSettings>
            <RunConfiguration>
            </RunConfiguration>
            {UnchangedDataCollectionRunSettings}
        </RunSettings>
        ";
            var expectedRunSettings = $@"
        <RunSettings>
            <RunConfiguration>
                <TestAdaptersPaths>MsTestAdapter</TestAdaptersPaths>
            </RunConfiguration>
            {UnchangedDataCollectionRunSettings}
        </RunSettings>";
            this.TestAddFCCSettings(
                runSettings,
                expectedRunSettings,
                new RunSettingsTemplateReplacements
                {
                    TestAdapter = "MsTestAdapter"
                }
            );

        }

        [Test]
        public void Should_Replace_TestAdaptersPath_If_Present()
        {
            var runSettings = $@"
        <RunSettings>
            <RunConfiguration>
                <TestAdaptersPaths>First;%fcc_testadapter%</TestAdaptersPaths>
            </RunConfiguration>
            {UnchangedDataCollectionRunSettings}
        </RunSettings>
        ";
            var expectedRunSettings = $@"
        <RunSettings>
            <RunConfiguration>
                <TestAdaptersPaths>First;MsTestAdapter</TestAdaptersPaths>
            </RunConfiguration>
            {UnchangedDataCollectionRunSettings}
        </RunSettings>";
            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements
            {
                TestAdapter = "MsTestAdapter"
            });
        }

        [Test]
        public void Should_Add_Replaceable_DataCollectionRunSettings_If_Not_Present()
        {
            var runSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
        </RunSettings>
        ";

            var expectedRunSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                <DataCollectors>
                    {MsDataCollectorIncludeCompanyNamesReplacements}
                </DataCollectors>
            </DataCollectionRunSettings>
        </RunSettings>
        ";

            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements
            {
                CompanyNamesInclude = "<CompanyName>Replacement</CompanyName>",
                Enabled = "true"
            });
        }

        [Test]
        public void Should_Add_Replaceable_DataCollectors_If_Not_Present()
        {
            var runSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>

            </DataCollectionRunSettings>
        </RunSettings>
        ";

            var expectedRunSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                <DataCollectors>
                    {MsDataCollectorIncludeCompanyNamesReplacements}
                </DataCollectors>
            </DataCollectionRunSettings>
        </RunSettings>
        ";

            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements
            {
                CompanyNamesInclude = "<CompanyName>Replacement</CompanyName>",
                Enabled = "true"
            });
        }

        [Test]
        public void Should_Add_Replaceable_Ms_Data_Collector_If_Not_Present()
        {
            var runSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                    <DataCollectors>
                        <DataCollector friendlyName='Other'/>
                    </DataCollectors>
                </DataCollectionRunSettings>
        </RunSettings>
        ";

            var expectedRunSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                <DataCollectors>
                    <DataCollector friendlyName='Other'/>
                    {MsDataCollectorIncludeCompanyNamesReplacements}
                </DataCollectors>
            </DataCollectionRunSettings>
        </RunSettings>
        ";

            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements
            {
                CompanyNamesInclude = "<CompanyName>Replacement</CompanyName>",
                Enabled = "true"
            });
        }

        [Test]
        public void Should_Replace_All_Replacements()
        {
            var runSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                    <DataCollectors>
                        <DataCollector friendlyName='Other'/>
                    </DataCollectors>
                </DataCollectionRunSettings>
        </RunSettings>
        ";

            var expectedRunSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                <DataCollectors>
                    <DataCollector friendlyName='Other'/>
                    <DataCollector friendlyName='Code Coverage' enabled='true'>
                        <Configuration>
                            <CodeCoverage>
                                <ModulePaths>
                                    <Exclude>
                                        <M>ExcludeReplacement</M>
                                    </Exclude>
                                    <Include>
                                        <M>IncludeReplacement</M>
                                    </Include>
                                </ModulePaths>
                                <Functions>
                                    <Exclude>
                                        <F>ExcludeReplacement</F>
                                    </Exclude>
                                    <Include>
                                        <F>IncludeReplacement</F>
                                    </Include>
                                </Functions>
                                <Attributes>
                                    <Exclude>
                                        <A>ExcludeReplacement</A>
                                    </Exclude>
                                    <Include>
                                        <A>IncludeReplacement</A>
                                    </Include>
                                </Attributes>
                                <Sources>
                                    <Exclude>
                                        <S>ExcludeReplacement</S>
                                    </Exclude>
                                    <Include>
                                        <S>IncludeReplacement</S>
                                    </Include>
                                </Sources>
                                <CompanyNames>
                                    <Exclude>
                                        <C>ExcludeReplacement</C>
                                    </Exclude>
                                    <Include>
                                        <C>IncludeReplacement</C>
                                    </Include>
                                </CompanyNames>
                                <PublicKeyTokens>
                                    <Exclude>
                                        <P>ExcludeReplacement</P>
                                    </Exclude>
                                    <Include>
                                        <P>IncludeReplacement</P>
                                    </Include>
                                </PublicKeyTokens>
                            </CodeCoverage>
                            <Format>Cobertura</Format>
                            <FCCGenerated/>
                        </Configuration>
            </DataCollector>
                </DataCollectors>
            </DataCollectionRunSettings>
        </RunSettings>
        ";

            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements
            {
                AttributesExclude = "<A>ExcludeReplacement</A>",
                AttributesInclude = "<A>IncludeReplacement</A>",
                CompanyNamesExclude = "<C>ExcludeReplacement</C>",
                CompanyNamesInclude = "<C>IncludeReplacement</C>",
                FunctionsExclude = "<F>ExcludeReplacement</F>",
                FunctionsInclude = "<F>IncludeReplacement</F>",
                ModulePathsExclude = "<M>ExcludeReplacement</M>",
                ModulePathsInclude = "<M>IncludeReplacement</M>",
                PublicKeyTokensExclude = "<P>ExcludeReplacement</P>",
                PublicKeyTokensInclude = "<P>IncludeReplacement</P>",
                SourcesExclude = "<S>ExcludeReplacement</S>",
                SourcesInclude = "<S>IncludeReplacement</S>",
                Enabled = "true"
            });
        }

        [Test]
        public void Should_Add_Missing_Format_Cobertura_To_Existing_Ms_Data_Collector()
        {
            var runSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                    <DataCollectors>
                        <DataCollector friendlyName='Code Coverage' enabled='true'>
                            <Configuration>
                                <CodeCoverage>
                                    <CompanyNames>
                                        <Include>
                                            %fcc_companynames_include%
                                            <CompanyName>Other</CompanyName>
                                        </Include>
                                    </CompanyNames>
                                </CodeCoverage>
                            </Configuration>
                        </DataCollector>
                    </DataCollectors>
                </DataCollectionRunSettings>
        </RunSettings>
        ";

            var expectedRunSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                <DataCollectors>
                    <DataCollector friendlyName='Code Coverage' enabled='true'>
                        <Configuration>
                            <CodeCoverage>
                                <CompanyNames>
                                    <Include>
                                        <CompanyName>Replacement</CompanyName>
                                        <CompanyName>Other</CompanyName>
                                    </Include>
                                </CompanyNames>
                            </CodeCoverage>
                            <Format>Cobertura</Format>
                        </Configuration>
                    </DataCollector>
                </DataCollectors>
            </DataCollectionRunSettings>
        </RunSettings>
        ";
            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements
            {
                CompanyNamesInclude = "<CompanyName>Replacement</CompanyName>",
                CompanyNamesExclude = "Not replaced",
                Enabled = "true"
            });
        }

        [Test]
        public void Should_Add_Missing_Configuration_Format_Cobertura_To_Existing_Ms_Data_Collector()
        {
            var runSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                    <DataCollectors>
                        <DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0' enabled='true'>
                        </DataCollector>
                    </DataCollectors>
                </DataCollectionRunSettings>
        </RunSettings>
        ";

            var expectedRunSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                <DataCollectors>
                    <DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0' enabled='true'>
                        <Configuration>
                            <Format>Cobertura</Format>
                        </Configuration>
                    </DataCollector>
                </DataCollectors>
            </DataCollectionRunSettings>
        </RunSettings>
        ";
            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements());
        }

        [Test]
        public void Should_Change_Format_To_Cobertura_For_Existing_Ms_Data_Collector()
        {
            var runSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                    <DataCollectors>
                        <DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0' enabled='true'>
                            <Configuration>
                                <Format>Xml</Format>
                            </Configuration>
                        </DataCollector>
                    </DataCollectors>
                </DataCollectionRunSettings>
        </RunSettings>
        ";

            var expectedRunSettings = $@"
        <RunSettings>
            {UnchangedRunConfiguration}
            <DataCollectionRunSettings>
                <DataCollectors>
                    <DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0' enabled='true'>
                        <Configuration>
                            <Format>Cobertura</Format>
                        </Configuration>
                    </DataCollector>
                </DataCollectors>
            </DataCollectionRunSettings>
        </RunSettings>
        ";

            this.TestAddFCCSettings(runSettings, expectedRunSettings, new RunSettingsTemplateReplacements());
        }

        [Test]
        public void Should_Add_Replaced_RunConfiguration_And_Add_Replaceable_DataCollectionRunSettings_If_Neither_Present()
        {
            var expectedRunSettings = @"
                <RunSettings>
                    <RunConfiguration>
                        <ResultsDirectory></ResultsDirectory>
                        <TestAdaptersPaths></TestAdaptersPaths>
                        <CollectSourceInformation>False</CollectSourceInformation>
                    </RunConfiguration>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                            <DataCollector friendlyName='Code Coverage' enabled='true'>
                                    <Configuration>
                                        <CodeCoverage>
                                            <ModulePaths>
                                                <Exclude></Exclude>
                                                <Include></Include>
                                            </ModulePaths>
                                            <Functions>
                                                <Exclude></Exclude>
                                                <Include></Include>
                                            </Functions>
                                            <Attributes>
                                                <Exclude></Exclude>
                                                <Include></Include>
                                            </Attributes>
                                            <Sources>
                                                <Exclude></Exclude>
                                                <Include></Include>
                                            </Sources>
                                            <CompanyNames>
                                                <Exclude></Exclude>
                                                <Include></Include>
                                            </CompanyNames>
                                            <PublicKeyTokens>
                                                <Exclude></Exclude>
                                                <Include></Include>
                                            </PublicKeyTokens>
                                        </CodeCoverage>
                                        <Format>Cobertura</Format>
                                        <FCCGenerated/>
                                    </Configuration>
                            </DataCollector>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>                
";
            this.TestAddFCCSettings("<RunSettings/>", expectedRunSettings, new RunSettingsTemplateReplacements { Enabled = "true" });
        }

        private void TestAddFCCSettings(string runSettings, string expectedFccRunSettings, IRunSettingsTemplateReplacements runSettingsTemplateReplacements)
        {
            var actualRunSettings = this.AddFCCSettings(runSettings, runSettingsTemplateReplacements);

            XmlAssert.NoXmlDifferences(actualRunSettings, expectedFccRunSettings);
        }


        private string AddFCCSettings(string runSettings, IRunSettingsTemplateReplacements runSettingsTemplateReplacements)
        {
            var xpathNavigable = XmlHelper.CreateXPathNavigable(runSettings);
            var mockRunSettingsTemplateReplacementsFactory = this.autoMocker.GetMock<IRunSettingsTemplateReplacementsFactory>();
            _ = mockRunSettingsTemplateReplacementsFactory.Setup(
                runSettingsTemplateReplacementsFactory => runSettingsTemplateReplacementsFactory.Create(
                    It.IsAny<IEnumerable<ITestContainer>>(),
                    It.IsAny<Dictionary<string, IUserRunSettingsProjectDetails>>(),
                    It.IsAny<string>()
                )
            ).Returns(runSettingsTemplateReplacements);
            return this.userRunSettingsService.AddFCCRunSettings(
                xpathNavigable, new Mock<IRunSettingsConfigurationInfo>().Object, null, null).DumpXmlContents();
        }


    }
}
