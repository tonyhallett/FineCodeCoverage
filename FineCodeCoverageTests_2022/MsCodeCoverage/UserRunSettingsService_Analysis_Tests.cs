namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using Moq;
    using NUnit.Framework;

    internal class MsCodeCoverage_UserRunSettingsService_Analysis_Tests
    {
        private AutoMoqer autoMocker;
        private UserRunSettingsService userRunSettingsService;
        private Mock<IFileUtil> mockFileUtil;

        [SetUp]
        public void SetUpSut()
        {
            this.autoMocker = new AutoMoqer();
            this.userRunSettingsService = this.autoMocker.Create<UserRunSettingsService>();
            this.mockFileUtil = this.autoMocker.GetMock<IFileUtil>();
        }

        [Test]
        public void Should_Be_Suitable_When_No_DataCollectors_Element_And_Use_MsCodeCoverage()
        {
            var (suitable, _) = this.ValidateUserRunSettings("<?xml version='1.0' encoding='utf-8'?><RunSettings/>", true);
            Assert.That(suitable, Is.True);
        }

        [Test]
        public void Should_Not_SpecifiedMsCodeCoverage_When_No_DataCollectors_Element()
        {
            var (_, specifiedMsCodeCoverage) = this.ValidateUserRunSettings("<?xml version='1.0' encoding='utf-8'?><RunSettings/>", true);
            Assert.That(specifiedMsCodeCoverage, Is.False);
        }

        [Test]
        public void Should_Be_UnSuitable_When_No_DataCollectors_Element_And_Use_MsCodeCoverage_False()
        {
            var (suitable, _) = this.ValidateUserRunSettings("<?xml version='1.0' encoding='utf-8'?><RunSettings/>", false);
            Assert.That(suitable, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Use_MsCodeCoverage_Dependent_When_DataCollectors_And_No_Ms_Collector(bool useMsCodeCoverage)
        {
            var (suitable, specifiedMsCodeCoverage) = this.ValidateUserRunSettings(
                "<?xml version='1.0' encoding='utf-8'?>" +
                @"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>
            ", useMsCodeCoverage);

            Assert.Multiple(() =>
            {
                Assert.That(suitable, Is.EqualTo(useMsCodeCoverage));
                Assert.That(specifiedMsCodeCoverage, Is.False);
            });
        }

        [Test]
        public void Should_Not_SpecifiedMsCodeCoverage_When_DataCollectors_And_No_Ms_Collector()
        {
            var (_, specifiedMsCodeCoverage) = this.ValidateUserRunSettings(
                "<?xml version='1.0' encoding='utf-8'?>" +
                @"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>
            ", true);
            Assert.That(specifiedMsCodeCoverage, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Suitable_And_Specified_When_Ms_Collector_FriendlyName_And_Cobertura_Format(bool useMsCodeCoverage)
        {
            var (suitable, specifiedMsCodeCoverage) = this.ValidateUserRunSettings(
                "<?xml version='1.0' encoding='utf-8'?>" +
                @"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                            <DataCollector friendlyName='Code Coverage'>
                                <Configuration>
                                    <Format>Cobertura</Format>
                                </Configuration>
                            </DataCollector>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>
            ", useMsCodeCoverage);

            Assert.Multiple(() =>
            {
                Assert.That(suitable, Is.True);
                Assert.That(specifiedMsCodeCoverage, Is.True);
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Suitable_And_Specified_When_Ms_Collector_Uri_And_Cobertura_Format(bool useMsCodeCoverage)
        {
            var (suitable, specifiedMsCodeCoverage) = this.ValidateUserRunSettings(
                "<?xml version='1.0' encoding='utf-8'?>" +
                @"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                            <DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0'>
                                <Configuration>
                                    <Format>Cobertura</Format>
                                </Configuration>
                            </DataCollector>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>
            ", useMsCodeCoverage);

            Assert.Multiple(() =>
            {
                Assert.That(suitable, Is.True);
                Assert.That(specifiedMsCodeCoverage, Is.True);
            });
        }

        [TestCase("uri='datacollector://Microsoft/CodeCoverage/2.0'", true)]
        [TestCase("uri='datacollector://Microsoft/CodeCoverage/2.0'", false)]
        [TestCase("friendlyName='Code Coverage'", true)]
        [TestCase("friendlyName='Code Coverage'", false)]
        public void Should_Be_Use_MsCodeCoverage_Dependent_When_Ms_Collector_With_No_Format(string collectorAttribute, bool useMsCodeCoverage)
        {
            var (suitable, specifiedMsCodeCoverage) = this.ValidateUserRunSettings(
                "<?xml version='1.0' encoding='utf-8'?>" +
                $@"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                            <DataCollector {collectorAttribute}>
                                <Configuration>
                                </Configuration>
                            </DataCollector>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>
            ", useMsCodeCoverage);

            Assert.Multiple(() =>
            {
                Assert.That(suitable, Is.EqualTo(useMsCodeCoverage));
                Assert.That(specifiedMsCodeCoverage, Is.True);
            });
        }

        [TestCase("uri='datacollector://Microsoft/CodeCoverage/2.0'", true)]
        [TestCase("uri='datacollector://Microsoft/CodeCoverage/2.0'", false)]
        [TestCase("friendlyName='Code Coverage'", true)]
        [TestCase("friendlyName='Code Coverage'", false)]
        public void Should_Be_Use_MsCodeCoverage_Dependent_When_Ms_Collector_With_Format_And_Not_Cobertura(string collectorAttribute, bool useMsCodeCoverage)
        {
            var (suitable, specifiedMsCodeCoverage) = this.ValidateUserRunSettings(
                "<?xml version='1.0' encoding='utf-8'?>" +
                $@"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                            <DataCollector {collectorAttribute}>
                                <Configuration>
                                    <Format>Xml</Format>
                                </Configuration>
                            </DataCollector>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>
            ", useMsCodeCoverage);

            Assert.Multiple(() =>
            {
                Assert.That(suitable, Is.EqualTo(useMsCodeCoverage));
                Assert.That(specifiedMsCodeCoverage, Is.True);
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Unsuitable_If_Invalid(bool useMsCodeCoverage)
        {
            var (suitable, _) = this.ValidateUserRunSettings("NotValid", useMsCodeCoverage);
            Assert.That(suitable, Is.False);
        }

        private (bool Suitable, bool SpecifiedMsCodeCoverage) ValidateUserRunSettings(string runSettings, bool useMsCodeCoverage)
        {
            var userRunSettingsPath = "some.runsettings";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText(userRunSettingsPath)).Returns(runSettings);
            return this.userRunSettingsService.ValidateUserRunSettings(userRunSettingsPath, useMsCodeCoverage);
        }

        [Test]
        public void Should_Be_Suitable_If_All_Are_Suitable()
        {
            var suitableXmlAsUseMsCodeCoverage = "<?xml version='1.0' encoding='utf-8'?><RunSettings/>";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText("Path1")).Returns(suitableXmlAsUseMsCodeCoverage);
            _ = this.mockFileUtil.Setup(f => f.ReadAllText("Path2")).Returns(suitableXmlAsUseMsCodeCoverage);

            var analysisResult = this.userRunSettingsService.Analyse(
                this.CreateCoverageProjectsWithRunSettings(new string[] { "Path1", "Path2" }),
                true,
                null
            );

            Assert.That(analysisResult.Suitable, Is.True);
            this.mockFileUtil.VerifyAll();
        }

        [Test]
        public void Should_Be_SpecifiedMsCodeCoverage_When_All_Suitable_And_Any_Specifies_Ms_Collector()
        {
            var suitableXmlAsUseMsCodeCoverage = "<?xml version='1.0' encoding='utf-8'?><RunSettings/>";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText("Path1")).Returns(suitableXmlAsUseMsCodeCoverage);

            var specifiesMsDataCollector = "<?xml version='1.0' encoding='utf-8'?>" +
                @"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                            <DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0'>
                                <Configuration>
                                    <Format>Cobertura</Format>
                                </Configuration>
                            </DataCollector>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText("Path2")).Returns(specifiesMsDataCollector);

            var analysisResult = this.userRunSettingsService.Analyse(
                this.CreateCoverageProjectsWithRunSettings(new string[] { "Path1", "Path2" }),
                true,
                null
            );

            Assert.That(analysisResult.SpecifiedMsCodeCoverage, Is.True);
        }

        [Test]
        public void Should_Be_SpecifiedMsCodeCoverage_False_When_All_Suitable_And_None_Specifies_Ms_Collector()
        {
            var suitableXmlAsUseMsCodeCoverage = "<?xml version='1.0' encoding='utf-8'?><RunSettings/>";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText("Path1")).Returns(suitableXmlAsUseMsCodeCoverage);

            var analysisResult = this.userRunSettingsService.Analyse(
                this.CreateCoverageProjectsWithRunSettings(new string[] { "Path1" }),
                true,
                null
            );

            Assert.That(analysisResult.SpecifiedMsCodeCoverage, Is.False);
        }

        [Test]
        public void Should_Be_Unsuitable_If_Any_Are_Unsuitable()
        {
            var suitableXmlAsUseMsCodeCoverage = "<?xml version='1.0' encoding='utf-8'?><RunSettings/>";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText("Path1")).Returns(suitableXmlAsUseMsCodeCoverage);
            var specifiesMsDataCollector = "<?xml version='1.0' encoding='utf-8'?>" +
                @"<RunSettings>
                    <DataCollectionRunSettings>
                        <DataCollectors>
                            <DataCollector uri='datacollector://Microsoft/CodeCoverage/2.0'>
                                <Configuration>
                                    <Format>Cobertura</Format>
                                </Configuration>
                            </DataCollector>
                        </DataCollectors>
                    </DataCollectionRunSettings>
                </RunSettings>";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText("Path2")).Returns(specifiesMsDataCollector);

            var analysisResult = this.userRunSettingsService.Analyse(
                this.CreateCoverageProjectsWithRunSettings(new string[] { "Path1", "Path2" }),
                false,
                null
            );

            Assert.Multiple(() =>
            {
                Assert.That(analysisResult.Suitable, Is.False);
                Assert.That(analysisResult.SpecifiedMsCodeCoverage, Is.False);
            });
        }

        [Test]
        public void Should_Have_Project_With_FCCMsTestAdapter_If_No_TestAdaptersPath()
        {
            var runSettingsNoTestAdaptersPath = "<?xml version='1.0' encoding='utf-8'?><RunSettings/>";
            var userRunSettingsPath = "some.runsettings";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText(userRunSettingsPath)).Returns(runSettingsNoTestAdaptersPath);
            var projectsWithTestAdapter = this.CreateCoverageProjectsWithRunSettings(userRunSettingsPath);
            var analysisResult = this.userRunSettingsService.Analyse(projectsWithTestAdapter, true, null);
            Assert.That(analysisResult.ProjectsWithFCCMsTestAdapter, Is.EqualTo(projectsWithTestAdapter));
        }

        [Test]
        public void Should_Have_Project_With_FCCMsTestAdapter_If_Has_Replaceable_Test_Adapter()
        {
            var mockRunSettingsTemplate = this.autoMocker.GetMock<IRunSettingsTemplate>();
            _ = mockRunSettingsTemplate.Setup(runSettingsTemplate => runSettingsTemplate.HasReplaceableTestAdapter("The paths")).Returns(true);
            var runSettingsTestAdaptersPath = @"<?xml version='1.0' encoding='utf-8'?>
                <RunSettings>
                    <RunConfiguration>
                        <TestAdaptersPaths>The paths</TestAdaptersPaths>
                    </RunConfiguration>
                </RunSettings>";

            var userRunSettingsPath = "some.runsettings";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText(userRunSettingsPath)).Returns(runSettingsTestAdaptersPath);
            var projectsWithTestAdapter = this.CreateCoverageProjectsWithRunSettings(userRunSettingsPath);

            var analysisResult = this.userRunSettingsService.Analyse(projectsWithTestAdapter, true, null);

            Assert.That(analysisResult.ProjectsWithFCCMsTestAdapter, Is.EqualTo(projectsWithTestAdapter));
        }

        [Test]
        public void Should_Have_Project_With_FCCMsTestAdapter_If_TestAdaptersPaths_Includes_The_FCC_Path()
        {
            var fccMsTestAdapterPath = "FCCPath";
            var runSettingsTestAdaptersPath = $@"<?xml version='1.0' encoding='utf-8'?>
                <RunSettings>
                    <RunConfiguration>
                        <TestAdaptersPaths>otherPath;{fccMsTestAdapterPath}</TestAdaptersPaths>
                    </RunConfiguration>
                </RunSettings>";

            var userRunSettingsPath = "some.runsettings";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText(userRunSettingsPath)).Returns(runSettingsTestAdaptersPath);
            var projectsWithTestAdapter = this.CreateCoverageProjectsWithRunSettings(userRunSettingsPath);

            var analysisResult = this.userRunSettingsService.Analyse(projectsWithTestAdapter, true, fccMsTestAdapterPath);

            Assert.That(analysisResult.ProjectsWithFCCMsTestAdapter, Is.EqualTo(projectsWithTestAdapter));
        }

        [Test]
        public void Should_Be_Possible_To_Have_Project_With_No_FCCMsTestAdapter()
        {
            var runSettingsTestAdaptersPath = $@"<?xml version='1.0' encoding='utf-8'?>
                <RunSettings>
                    <RunConfiguration>
                        <TestAdaptersPaths>otherPath;</TestAdaptersPaths>
                    </RunConfiguration>
                </RunSettings>";

            var userRunSettingsPath = "some.runsettings";
            _ = this.mockFileUtil.Setup(f => f.ReadAllText(userRunSettingsPath)).Returns(runSettingsTestAdaptersPath);
            var projectsWithTestAdapter = this.CreateCoverageProjectsWithRunSettings(userRunSettingsPath);

            var analysisResult = this.userRunSettingsService.Analyse(projectsWithTestAdapter, true, "FCCPath");

            Assert.That(analysisResult.ProjectsWithFCCMsTestAdapter, Is.Empty);
        }

        private List<ICoverageProject> CreateCoverageProjectsWithRunSettings(params string[] runSettingsPaths) =>
            runSettingsPaths.Select(path =>
            {
                var mock = new Mock<ICoverageProject>();
                _ = mock.Setup(cp => cp.RunSettingsFile).Returns(path);
                return mock.Object;
            }).ToList();
    }
}
