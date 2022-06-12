namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverageTests.MsCodeCoverage_Tests.Helpers;
    using Moq;
    using NUnit.Framework;

    internal class TestCoverageProjectRunSettings : ICoverageProjectRunSettings
    {
        public TestCoverageProjectRunSettings(Guid id, string outputFolder, string projectName, string runSettings)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Id).Returns(id);
            _ = mockCoverageProject.Setup(cp => cp.ProjectName).Returns(projectName);
            _ = mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns(outputFolder);
            this.CoverageProject = mockCoverageProject.Object;

            this.RunSettings = runSettings;
        }
        public ICoverageProject CoverageProject { get; set; }
        public string RunSettings { get; set; }
    }

    internal class ProjectRunSettingsGenerator_Tests
    {
        private AutoMoqer autoMocker;
        private ProjectRunSettingsGenerator projectRunSettingsGenerator;
        private Guid projectId1;
        private Guid projectId2;
        private List<ICoverageProjectRunSettings> coverageProjectsRunSettings;
        private string generatedRunSettingsInOutputFolderPath1;
        private string generatedRunSettingsInOutputFolderPath2;
        private const string RunSettings1 = XmlHelper.XmlDeclaration + @"
            <RunSettings>
<RunConfiguration/>
            </RunSettings>
";
        private const string RunSettings2 = XmlHelper.XmlDeclaration + @"
            <RunSettings>
                        <RunConfiguration/>
            </RunSettings>
";

        [SetUp]
        public void Setup()
        {
            this.autoMocker = new AutoMoqer();
            this.projectRunSettingsGenerator = this.autoMocker.Create<ProjectRunSettingsGenerator>();
            this.projectId1 = Guid.NewGuid();
            this.projectId2 = Guid.NewGuid();
            this.coverageProjectsRunSettings = new List<ICoverageProjectRunSettings>
            {
                new TestCoverageProjectRunSettings(this.projectId1, "OutputFolder1","Project1",RunSettings1),
                new TestCoverageProjectRunSettings(this.projectId2, "OutputFolder2","Project2",RunSettings2),
            };
            this.generatedRunSettingsInOutputFolderPath1 = Path.Combine(
                "OutputFolder1", "Project1-fcc-mscodecoverage-generated.runsettings"
            );
            this.generatedRunSettingsInOutputFolderPath2 = Path.Combine(
                "OutputFolder2", "Project2-fcc-mscodecoverage-generated.runsettings"
            );
        }

        [Test]
        public async Task Should_Write_All_Project_Run_Settings_File_Path_With_The_VsRunSettingsWriter_Async()
        {
            await this.projectRunSettingsGenerator.WriteProjectsRunSettingsAsync(this.coverageProjectsRunSettings);

            var mockVsRunSettingsWriter = this.autoMocker.GetMock<IVsRunSettingsWriter>();

            mockVsRunSettingsWriter.Verify(
                rsw => rsw.WriteRunSettingsFilePathAsync(this.projectId1, this.generatedRunSettingsInOutputFolderPath1)
            );
            mockVsRunSettingsWriter.Verify(
                rsw => rsw.WriteRunSettingsFilePathAsync(this.projectId2, this.generatedRunSettingsInOutputFolderPath2)
            );
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Write_RunSettings_In_Project_Output_Folder_If_The_VsRunSettingsWriter_Is_Successful_Async(bool success)
        {
            var mockVsRunSettingsWriter = this.autoMocker.GetMock<IVsRunSettingsWriter>();
            _ = mockVsRunSettingsWriter.Setup(rsw =>
                    rsw.WriteRunSettingsFilePathAsync(this.projectId1, this.generatedRunSettingsInOutputFolderPath1)
            ).ReturnsAsync(success);
            _ = mockVsRunSettingsWriter.Setup(
                rsw => rsw.WriteRunSettingsFilePathAsync(this.projectId2, this.generatedRunSettingsInOutputFolderPath2)
            ).ReturnsAsync(success);

            var mockFileUtil = this.autoMocker.GetMock<IFileUtil>();
            await this.projectRunSettingsGenerator.WriteProjectsRunSettingsAsync(this.coverageProjectsRunSettings);
            if (success)
            {
                var prettyRunSettings1 = XDocument.Parse(RunSettings1).FormatXml();
                var prettyRunSettings2 = XDocument.Parse(RunSettings2).FormatXml();
                mockFileUtil.Verify(f => f.WriteAllText(this.generatedRunSettingsInOutputFolderPath1, prettyRunSettings1));
                mockFileUtil.Verify(f => f.WriteAllText(this.generatedRunSettingsInOutputFolderPath2, prettyRunSettings2));
            }
            else
            {
                mockFileUtil.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            }
        }

        [Test]
        public async Task Should_Remove_Generated_Run_Settings_File_Path_With_The_VsRunSettingsWriter_Async()
        {
            var mockProjectWithGeneratedRunSettings = new Mock<ICoverageProject>();
            var mockProjectWithoutRunSettings = new Mock<ICoverageProject>();
            var mockProjectWithoutGeneratedRunSettings = new Mock<ICoverageProject>();


            ICoverageProject SetupProject(Mock<ICoverageProject> mockCoverageProject, string runSettingsFilePath, Guid id)
            {
                _ = mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns(runSettingsFilePath);
                _ = mockCoverageProject.Setup(cp => cp.Id).Returns(id);
                return mockCoverageProject.Object;
            }

            var p1 = SetupProject(
                mockProjectWithGeneratedRunSettings, "Project1-fcc-mscodecoverage-generated.runsettings", this.projectId1
            );
            var p2 = SetupProject(mockProjectWithoutRunSettings, null, this.projectId2);
            var p3 = SetupProject(mockProjectWithoutGeneratedRunSettings, "", Guid.NewGuid());

            await this.projectRunSettingsGenerator.RemoveGeneratedProjectSettingsAsync(new ICoverageProject[] { p1, p2, p3 });

            var mockVsRunSettingsWriter = this.autoMocker.GetMock<IVsRunSettingsWriter>();
            mockVsRunSettingsWriter.Verify(rsw => rsw.RemoveRunSettingsFilePathAsync(this.projectId1));
            mockVsRunSettingsWriter.VerifyNoOtherCalls();
        }
    }
}
