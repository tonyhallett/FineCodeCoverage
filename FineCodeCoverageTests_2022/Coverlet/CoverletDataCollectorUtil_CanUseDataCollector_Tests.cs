namespace FineCodeCoverageTests.Coverlet_Tests
{
    using System;
    using System.Xml.Linq;
    using AutoMoq;
    using FineCodeCoverage.Core.Coverlet;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Coverlet;
    using FineCodeCoverage.Engine.Model;
    using Moq;
    using NUnit.Framework;

    public class CoverletDataCollectorUtil_CanUseDataCollector_Tests
    {
        private AutoMoqer mocker;
        private CoverletDataCollectorUtil coverletDataCollectorUtil;

        public enum UseDataCollectorElement { True, False, Empty, None }

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.coverletDataCollectorUtil = this.mocker.Create<CoverletDataCollectorUtil>();
            this.coverletDataCollectorUtil.ThreadHelper = new TestThreadHelper();
        }

        private void SetUpRunSettings(Mock<ICoverageProject> mockCoverageProject, Action<Mock<IRunSettingsCoverletConfiguration>> setup)
        {
            _ = mockCoverageProject.Setup(p => p.RunSettingsFile).Returns(".runsettings");
            var mockRunSettingsCoverletConfiguration = this.mocker.GetMock<IRunSettingsCoverletConfiguration>();
            var mockRunSettingsCoverletConfigurationFactory = this.mocker.GetMock<IRunSettingsCoverletConfigurationFactory>();
            _ = mockRunSettingsCoverletConfigurationFactory.Setup(rscf => rscf.Create()).Returns(mockRunSettingsCoverletConfiguration.Object);
            setup?.Invoke(mockRunSettingsCoverletConfiguration);
        }

        private void SetupDataCollectorState(Mock<ICoverageProject> mockCoverageProject, CoverletDataCollectorState coverletDataCollectorState) =>
            this.SetUpRunSettings(mockCoverageProject, mrsc => mrsc.Setup(rsc => rsc.CoverletDataCollectorState)
            .Returns(coverletDataCollectorState));

        private XElement GetProjectElementWithDataCollector(UseDataCollectorElement useDataCollector)
        {
            var useDataCollectorElement = "";
            if (useDataCollector != UseDataCollectorElement.None)
            {
                var value = "";
                if (useDataCollector == UseDataCollectorElement.True)
                {
                    value = "true";
                }
                if (useDataCollector == UseDataCollectorElement.False)
                {
                    value = "false";
                }
                useDataCollectorElement = $"<PropertyGroup><UseDataCollector>{value}</UseDataCollector></PropertyGroup>";
            }

            return XElement.Parse($@"<Project>
{useDataCollectorElement}
</Project>");
        }


        [Test]
        public void Should_Factory_Create_Configuration_And_Read_CoverageProject_RunSettings()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement)
                .Returns(this.GetProjectElementWithDataCollector(UseDataCollectorElement.True));
            var runSettingsFilePath = ".runsettings";
            _ = mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns(runSettingsFilePath);

            var settingsXml = "<settings../>";
            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(f => f.ReadAllText(runSettingsFilePath)).Returns(settingsXml);

            var mockRunSettingsCoverletConfiguration = new Mock<IRunSettingsCoverletConfiguration>();
            var mockRunSettingsCoverletConfigurationFactory = this.mocker.GetMock<IRunSettingsCoverletConfigurationFactory>();
            _ = mockRunSettingsCoverletConfigurationFactory.Setup(rscf => rscf.Create()).Returns(mockRunSettingsCoverletConfiguration.Object);
            _ = this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object);

            mockRunSettingsCoverletConfigurationFactory.VerifyAll();
            mockRunSettingsCoverletConfiguration.Verify(rsc => rsc.Read(settingsXml));

        }

        [TestCase(UseDataCollectorElement.None)]
        [TestCase(UseDataCollectorElement.True)]
        public void Should_Use_Data_Collector_If_RunSettings_Has_The_Data_Collector_Enabled_And_Not_Overridden_By_Project_File(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement)
                .Returns(this.GetProjectElementWithDataCollector(useDataCollector));
            this.SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.Enabled);

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object));
        }

        [Test]
        public void Should_Not_Use_Data_Collector_If_RunSettings_Has_The_Data_Collector_Enabled_And_Overridden_By_Project_File()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement)
                .Returns(this.GetProjectElementWithDataCollector(UseDataCollectorElement.False));

            this.SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.Enabled);

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object), Is.False);
        }

        [TestCase(UseDataCollectorElement.True)]
        [TestCase(UseDataCollectorElement.Empty)]
        public void Should_Use_Data_Collector_If_Not_Specified_In_RunSettings_And_Specified_In_ProjectFile(UseDataCollectorElement useDataCollectorElement)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement)
                .Returns(this.GetProjectElementWithDataCollector(useDataCollectorElement));

            this.SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.NotPresent);

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object), Is.True);
        }

        [TestCase(UseDataCollectorElement.True, true)]
        [TestCase(UseDataCollectorElement.Empty, true)]
        [TestCase(UseDataCollectorElement.False, false)]
        [TestCase(UseDataCollectorElement.None, false)]
        public void Should_Use_Data_Collector_If_Not_Specified_In_RunSettings_And_Specified_In_ProjectFile_VSBuild(UseDataCollectorElement useDataCollector, bool expected)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            var guid = Guid.NewGuid();
            _ = mockCoverageProject.Setup(cp => cp.Id).Returns(guid);
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(new XElement("Root"));


            var mockVsBuildFCCSettingsProvider = this.mocker.GetMock<IVsBuildFCCSettingsProvider>();
            var useDataCollectorElement = "";
            if (useDataCollector != UseDataCollectorElement.None)
            {
                var value = "";
                if (useDataCollector == UseDataCollectorElement.True)
                {
                    value = "true";
                }
                if (useDataCollector == UseDataCollectorElement.False)
                {
                    value = "false";
                }
                useDataCollectorElement = $"<UseDataCollector>{value}</UseDataCollector>";
            }
            var vsBuildProjectFileElement = XElement.Parse($"<FineCodeCoverage>{useDataCollectorElement}</FineCodeCoverage>");
            _ = mockVsBuildFCCSettingsProvider.Setup(
                vsBuildFCCSettingsProvider =>
                vsBuildFCCSettingsProvider.GetSettingsAsync(guid)
            ).ReturnsAsync(vsBuildProjectFileElement);

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object), Is.EqualTo(expected));
        }

        [Test]
        public void Should_Use_Data_Collector_If_No_RunSettings_And_Specified_In_ProjectFile()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns((string)null);
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement)
                .Returns(this.GetProjectElementWithDataCollector(UseDataCollectorElement.True));

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object), Is.True);
        }


        [TestCase(UseDataCollectorElement.False)]
        [TestCase(UseDataCollectorElement.None)]
        public void Should_Not_Use_Data_Collector_If_Not_Specified_In_RunSettings_And_Not_Specified_In_ProjectFile(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement)
                .Returns(this.GetProjectElementWithDataCollector(useDataCollector));
            this.SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.NotPresent);

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object), Is.False);
        }

        [TestCase(UseDataCollectorElement.False)]
        [TestCase(UseDataCollectorElement.None)]
        public void Should_Not_Use_Data_Collector_If_No_RunSettings_And_Not_Specified_In_ProjectFile(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns((string)null);
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(this.GetProjectElementWithDataCollector(useDataCollector));

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object), Is.False);
        }

        [TestCase(UseDataCollectorElement.True)]
        [TestCase(UseDataCollectorElement.False)]
        [TestCase(UseDataCollectorElement.None)]
        [TestCase(UseDataCollectorElement.Empty)]
        public void Should_Not_Use_Data_Collector_If_Disabled_In_RunSettings(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement)
                .Returns(this.GetProjectElementWithDataCollector(useDataCollector));
            this.SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.Disabled);

            Assert.That(this.coverletDataCollectorUtil.CanUseDataCollector(mockCoverageProject.Object), Is.False);
        }

    }
}
