namespace FineCodeCoverageTests.Coverlet_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;
    using AutoMoq;
    using FineCodeCoverage.Engine.Coverlet;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;
    using Org.XmlUnit.Builder;

    internal class DataCollectorSettingsBuilder_Tests
    {
        private AutoMoqer mocker;
        private DataCollectorSettingsBuilder dataCollectorSettingsBuilder;
        private string generatedRunSettingsPath;
        private string existingRunSettingsPath;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.dataCollectorSettingsBuilder = this.mocker.Create<DataCollectorSettingsBuilder>();
            this.generatedRunSettingsPath = Path.GetTempFileName();
            this.existingRunSettingsPath = Path.GetTempFileName();
        }

        [TearDown]
        public void DeleteRunSettings()
        {
            File.Delete(this.generatedRunSettingsPath);
            File.Delete(this.existingRunSettingsPath);
        }

        private void Initialize(bool runSettingsOnly = false)
        {
            var mockCoverageProjectSettings = new Mock<IAppOptions>();
            _ = mockCoverageProjectSettings.Setup(o => o.RunSettingsOnly).Returns(runSettingsOnly);
            this.dataCollectorSettingsBuilder.Initialize(mockCoverageProjectSettings.Object, this.existingRunSettingsPath, this.generatedRunSettingsPath);
        }

        #region arguments

        [Test]
        public void Should_Safely_Quote_Paths_When_Quote()
        {
            var quoted = this.dataCollectorSettingsBuilder.Quote(@"C\Some Path");
            Assert.That(quoted, Is.EqualTo(@"""C\Some Path"""));
        }

        [Test]
        public void Should_Set_Blame_Flag_When_WithBlame()
        {
            this.dataCollectorSettingsBuilder.WithBlame();
            Assert.That(this.dataCollectorSettingsBuilder.Blame, Is.EqualTo("--blame"));
        }

        [Test]
        public void Should_Set_NoLogo_Flag_When_WithNoLogo()
        {
            this.dataCollectorSettingsBuilder.WithNoLogo();
            Assert.That(this.dataCollectorSettingsBuilder.NoLogo, Is.EqualTo("--nologo"));
        }

        [Test]
        public void Should_Set_Diagnostics_Flag_Quoted_When_WithDiagnostics()
        {
            this.dataCollectorSettingsBuilder.WithDiagnostics("path");
            Assert.That($"--diag {this.dataCollectorSettingsBuilder.Quote("path")}", Is.EqualTo(this.dataCollectorSettingsBuilder.Diagnostics));
        }

        [Test]
        public void Should_Set_Results_Directory_Flag_Quoted_When_WithResultsDirectory()
        {
            this.dataCollectorSettingsBuilder.WithResultsDirectory("path");
            Assert.That($"--results-directory {this.dataCollectorSettingsBuilder.Quote("path")}", Is.EqualTo(this.dataCollectorSettingsBuilder.ResultsDirectory));
        }

        [Test]
        public void Should_Set_ProjectDll_Quoted_When_WithProjectDll()
        {
            this.dataCollectorSettingsBuilder.WithProjectDll("projectdll");
            Assert.That(this.dataCollectorSettingsBuilder.Quote("projectdll"), Is.EqualTo(this.dataCollectorSettingsBuilder.ProjectDll));
        }

        [Test]
        public void Should_Set_RunSettings_As_Quoted_GeneratedRunSettings_When_Initialize()
        {
            this.dataCollectorSettingsBuilder.Initialize(null, ".runsettings", "generated.runsettings");
            Assert.That($"--settings {this.dataCollectorSettingsBuilder.Quote("generated.runsettings")}", Is.EqualTo(this.dataCollectorSettingsBuilder.RunSettings));
        }

        #endregion

        [Test]
        public void Should_Have_Format_As_Cobertura() =>
            Assert.That(this.dataCollectorSettingsBuilder.Format, Is.EqualTo("cobertura"));

        [Test]
        public void Should_Use_RunSettings_Exclude_If_Present()
        {
            this.Initialize();
            this.dataCollectorSettingsBuilder.WithExclude(new string[] { "from project" }, "from run settings");
            Assert.That(this.dataCollectorSettingsBuilder.Exclude, Is.EqualTo("from run settings"));
        }

        [Test]
        public void Should_Use_Project_Exclude_If_No_RunSettings()
        {
            this.dataCollectorSettingsBuilder.WithExclude(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.Exclude, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Use_Project_Exclude_If_No_RunSettings_Null()
        {
            this.dataCollectorSettingsBuilder.WithExclude(null, null);
            Assert.That(this.dataCollectorSettingsBuilder.Exclude, Is.Null);
        }

        [Test]
        public void Should_Fallback_To_Options_For_Exclude_If_Not_RunSettingsOnly()
        {
            this.Initialize(false);
            this.dataCollectorSettingsBuilder.WithExclude(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.Exclude, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Not_Fallback_To_Options_For_Exclude_If_RunSettingsOnly()
        {
            this.Initialize(true);
            this.dataCollectorSettingsBuilder.WithExclude(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.Exclude, Is.Null);
        }

        [Test]
        public void Should_Use_RunSettings_ExcludeByAttribute_If_Present()
        {
            this.Initialize();
            this.dataCollectorSettingsBuilder.WithExcludeByAttribute(new string[] { "from project" }, "from run settings");
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByAttribute, Is.EqualTo("from run settings"));
        }

        [Test]
        public void Should_Use_Project_ExcludeByAttribute_If_No_RunSettings()
        {
            this.dataCollectorSettingsBuilder.WithExcludeByAttribute(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByAttribute, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Use_Project_ExcludeByAttribute_If_No_RunSettings_Null()
        {
            this.dataCollectorSettingsBuilder.WithExcludeByAttribute(null, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByAttribute, Is.Null);
        }

        [Test]
        public void Should_Fallback_To_Options_For_ExcludeByAttribute_If_Not_RunSettingsOnly()
        {
            this.Initialize(false);
            this.dataCollectorSettingsBuilder.WithExcludeByAttribute(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByAttribute, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Not_Fallback_To_Options_For_ExcludeByAttribute_If_RunSettingsOnly()
        {
            this.Initialize(true);
            this.dataCollectorSettingsBuilder.WithExcludeByAttribute(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByAttribute, Is.Null);
        }

        [Test]
        public void Should_Use_RunSettings_ExcludeByFile_If_Present()
        {
            this.Initialize();
            this.dataCollectorSettingsBuilder.WithExcludeByFile(new string[] { "from project" }, "from run settings");
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByFile, Is.EqualTo("from run settings"));
        }

        [Test]
        public void Should_Use_Project_ExcludeByFile_If_No_RunSettings()
        {
            this.dataCollectorSettingsBuilder.WithExcludeByFile(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByFile, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Use_Project_ExcludeByFile_If_No_RunSettings_Null()
        {
            this.dataCollectorSettingsBuilder.WithExcludeByFile(null, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByFile, Is.Null);
        }

        [Test]
        public void Should_Fallback_To_Options_For_ExcludeByFile_If_Not_RunSettingsOnly()
        {
            this.Initialize(false);
            this.dataCollectorSettingsBuilder.WithExcludeByFile(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByFile, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Not_Fallback_To_Options_For_ExcludeByFile_If_RunSettingsOnly()
        {
            this.Initialize(true);
            this.dataCollectorSettingsBuilder.WithExcludeByFile(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.ExcludeByFile, Is.Null);
        }

        [Test]
        public void Should_Use_RunSettings_Include_If_Present()
        {
            this.Initialize();
            this.dataCollectorSettingsBuilder.WithInclude(new string[] { "from project" }, "from run settings");
            Assert.That(this.dataCollectorSettingsBuilder.Include, Is.EqualTo("from run settings"));
        }

        [Test]
        public void Should_Use_Project_Include_If_No_RunSettings()
        {
            this.dataCollectorSettingsBuilder.WithInclude(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.Include, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Use_Project_Include_If_No_RunSettings_Null()
        {
            this.dataCollectorSettingsBuilder.WithInclude(null, null);
            Assert.That(this.dataCollectorSettingsBuilder.Include, Is.Null);
        }

        [Test]
        public void Should_Fallback_To_Options_For_Include_If_Not_RunSettingsOnly()
        {
            this.Initialize(false);
            this.dataCollectorSettingsBuilder.WithInclude(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.Include, Is.EqualTo("first,second"));
        }

        [Test]
        public void Should_Not_Fallback_To_Options_For_Include_If_RunSettingsOnly()
        {
            this.Initialize(true);
            this.dataCollectorSettingsBuilder.WithInclude(new string[] { "first", "second" }, null);
            Assert.That(this.dataCollectorSettingsBuilder.Include, Is.Null);
        }

        [TestCase("true")]
        [TestCase("false")]
        public void Should_Use_RunSettings_IncludeTestAssembly_If_Present(string runSettingsIncludeTestAssembly)
        {
            this.Initialize();
            this.dataCollectorSettingsBuilder.WithIncludeTestAssembly(true, runSettingsIncludeTestAssembly);
            Assert.That(this.dataCollectorSettingsBuilder.IncludeTestAssembly, Is.EqualTo(runSettingsIncludeTestAssembly));
        }

        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void Should_Use_Project_IncludeTestAssembly_If_No_RunSettings(bool optionsIncludeTestAssembly, string expected)
        {
            this.dataCollectorSettingsBuilder.WithIncludeTestAssembly(optionsIncludeTestAssembly, null);
            Assert.That(this.dataCollectorSettingsBuilder.IncludeTestAssembly, Is.EqualTo(expected));
        }

        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void Should_Fallback_To_Options_For_IncludeTestAssembly_If_Not_RunSettingsOnly(bool optionsIncludeTestAssembly, string expected)
        {
            this.Initialize(false);
            this.dataCollectorSettingsBuilder.WithIncludeTestAssembly(optionsIncludeTestAssembly, null);
            Assert.That(expected, Is.EqualTo(this.dataCollectorSettingsBuilder.IncludeTestAssembly));
        }

        [Test]
        public void Should_Not_Fallback_To_Options_For_IncludeTestAssembly_If_RunSettingsOnly()
        {
            this.Initialize(true);
            this.dataCollectorSettingsBuilder.WithIncludeTestAssembly(true, null);
            Assert.That(this.dataCollectorSettingsBuilder.Include, Is.Null);
        }

        [Test]
        public void Should_Set_Corresponding_Property_For_RunSettings_Only_Elements()
        {
            this.dataCollectorSettingsBuilder.WithSingleHit("singlehit");
            this.dataCollectorSettingsBuilder.WithSkipAutoProps("skipautoprops");
            this.dataCollectorSettingsBuilder.WithIncludeDirectory("includedirectory");
            this.dataCollectorSettingsBuilder.WithUseSourceLink("sourcelink");

            Assert.Multiple(() =>
            {
                Assert.That(this.dataCollectorSettingsBuilder.SingleHit, Is.EqualTo("singlehit"));
                Assert.That(this.dataCollectorSettingsBuilder.SkipAutoProps, Is.EqualTo("skipautoprops"));
                Assert.That(this.dataCollectorSettingsBuilder.IncludeDirectory, Is.EqualTo("includedirectory"));
                Assert.That(this.dataCollectorSettingsBuilder.UseSourceLink, Is.EqualTo("sourcelink"));
            });
        }

        [TestCaseSource(nameof(BuildReturnsSettingsSource))]
        public void Should_Return_Dotnet_Test_Settings_When_Build(Action<DataCollectorSettingsBuilder> setUp, string expectedSettings)
        {
            this.dataCollectorSettingsBuilder.Initialize(new Mock<IAppOptions>().Object, null, this.generatedRunSettingsPath);
            setUp(this.dataCollectorSettingsBuilder);
            Assert.That(expectedSettings, Is.EqualTo(this.dataCollectorSettingsBuilder.Build()));
        }

        private static IEnumerable<TestCaseData> BuildReturnsSettingsSource()
        {
            Action<DataCollectorSettingsBuilder> allSettingsSetup = builder =>
            {
                builder.ProjectDll = "test.dll";
                builder.Blame = "blame";
                builder.NoLogo = "nologo";
                builder.Diagnostics = "diagnostics";
                builder.RunSettings = "runsettings";
                builder.ResultsDirectory = "resultsdirectory";
            };
            var allSettingsExpected = "test.dll blame nologo diagnostics runsettings resultsdirectory";
            return new List<TestCaseData>
            {
                new TestCaseData(allSettingsSetup, allSettingsExpected)
            };
        }

        [TestCaseSource(nameof(BuildGeneratesRunSettingsSource))]
        public void Should_Generate_RunSettings_When_Builds(Action<DataCollectorSettingsBuilder> setUp, string existingRunSettings, string expectedXml)
        {
            setUp(this.dataCollectorSettingsBuilder);

            if (existingRunSettings != null)
            {
                XDocument.Parse(existingRunSettings).Save(this.existingRunSettingsPath);
            }

            this.dataCollectorSettingsBuilder.Initialize(null, existingRunSettings == null ? null : this.existingRunSettingsPath, this.generatedRunSettingsPath);
            _ = this.dataCollectorSettingsBuilder.Build();


            var diff = DiffBuilder.Compare(Input.FromDocument(XDocument.Load(this.generatedRunSettingsPath)))
             .WithTest(Input.From(expectedXml)).Build();

            Assert.That(diff.HasDifferences(), Is.False);

        }

        private static IEnumerable<TestCaseData> BuildGeneratesRunSettingsSource()
        {
            Action<DataCollectorSettingsBuilder> fullSetup = (dataCollectorSettingsBuilder) =>
            {
                dataCollectorSettingsBuilder.Format = "coverlet";
                dataCollectorSettingsBuilder.Exclude = "exclude";
                dataCollectorSettingsBuilder.Include = "include";
                dataCollectorSettingsBuilder.ExcludeByAttribute = "excludebyattribute";
                dataCollectorSettingsBuilder.ExcludeByFile = "excludebyfile";
                dataCollectorSettingsBuilder.IncludeTestAssembly = "includetestassembly";
                dataCollectorSettingsBuilder.IncludeDirectory = "includedirectory";
                dataCollectorSettingsBuilder.SingleHit = "singlehit";
                dataCollectorSettingsBuilder.UseSourceLink = "sourcelink";
                dataCollectorSettingsBuilder.SkipAutoProps = "skipautoprops";
            };

            var expectedXml = @"<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat Code Coverage"">
                <Configuration>
                    <Format>coverlet</Format>
                    <Exclude>exclude</Exclude>
                    <Include>include</Include>
                    <ExcludeByAttribute>excludebyattribute</ExcludeByAttribute>
                    <ExcludeByFile>excludebyfile</ExcludeByFile>
                    <IncludeDirectory>includedirectory</IncludeDirectory>
                    <SingleHit>singlehit</SingleHit>
                    <UseSourceLink>sourcelink</UseSourceLink>
                    <IncludeTestAssembly>includetestassembly</IncludeTestAssembly>
                    <SkipAutoProps>skipautoprops</SkipAutoProps>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>
";

            var noExistingRunSettingsTest = new TestCaseData(fullSetup, null, expectedXml);

            var existingCoverletCollector = @"<RunSettings>
      <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat Code Coverage"">
                <Configuration>
                    <Format>json</Format>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>
";

            var withExistingReplaceDataCollectorTest = new TestCaseData(fullSetup, existingCoverletCollector, expectedXml);

            var noDataCollectionRunSettings = @"<RunSettings>
</RunSettings>
";

            var noDataCollectionRunSettingsTest = new TestCaseData(fullSetup, noDataCollectionRunSettings, expectedXml);

            var noDataCollectors = @"<RunSettings>
      <DataCollectionRunSettings>
    </DataCollectionRunSettings>
</RunSettings>
";
            var noDataCollectorsTest = new TestCaseData(fullSetup, noDataCollectors, expectedXml);

            var noCoverletCollector = @"<RunSettings>
      <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""Other"">
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>
";

            var noCoverletCollectorExpectedXml = @"<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""Other"">
            </DataCollector>
            <DataCollector friendlyName=""XPlat Code Coverage"">
                <Configuration>
                    <Format>coverlet</Format>
                    <Exclude>exclude</Exclude>
                    <Include>include</Include>
                    <ExcludeByAttribute>excludebyattribute</ExcludeByAttribute>
                    <ExcludeByFile>excludebyfile</ExcludeByFile>
                    <IncludeDirectory>includedirectory</IncludeDirectory>
                    <SingleHit>singlehit</SingleHit>
                    <UseSourceLink>sourcelink</UseSourceLink>
                    <IncludeTestAssembly>includetestassembly</IncludeTestAssembly>
                    <SkipAutoProps>skipautoprops</SkipAutoProps>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>
";

            var noCoverletCollectorTest = new TestCaseData(fullSetup, noCoverletCollector, noCoverletCollectorExpectedXml);

            Action<DataCollectorSettingsBuilder> partialSetup =
                (dataCollectorSettingsBuilder) => dataCollectorSettingsBuilder.Format = "coverlet";

            var expectedNullElementXml = @"<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName=""XPlat Code Coverage"">
                <Configuration>
                    <Format>coverlet</Format>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>
";

            var doesNotSetElementsWhenNullTest = new TestCaseData(partialSetup, null, expectedNullElementXml);

            return new List<TestCaseData>
            {
                noExistingRunSettingsTest,
                withExistingReplaceDataCollectorTest,
                noDataCollectionRunSettingsTest,
                noDataCollectorsTest,
                noCoverletCollectorTest,
                doesNotSetElementsWhenNullTest
            };
        }
    }
}
