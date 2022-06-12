namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using Moq;
    using NUnit.Framework;

    internal class TemplatedRunSettingsService_Tests
    {
        private AutoMoqer autoMocker;
        private TemplatedRunSettingsService templatedRunSettingsService;

        [SetUp]
        public void SetupSut()
        {
            this.autoMocker = new AutoMoqer();
            this.templatedRunSettingsService = this.autoMocker.Create<TemplatedRunSettingsService>();
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Create_Run_Settings_From_Template_Async(bool isDotNetFramework)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.SetupGet(cp => cp.IsDotNetFramework).Returns(isDotNetFramework);
            var coverageProject = mockCoverageProject.Object;
            var coverageProjects = new List<ICoverageProject> { coverageProject };

            var mockRunSettingsTemplate = this.autoMocker.GetMock<IRunSettingsTemplate>();
            _ = mockRunSettingsTemplate.Setup(runSettingsTemplate => runSettingsTemplate.ToString()).Returns("<MockRunSettingsTemplate/>");

            var runSettingsTemplateReplacements = new RunSettingsTemplateReplacements();
            var mockRunSettingsTemplateReplacementFactory = this.autoMocker.GetMock<IRunSettingsTemplateReplacementsFactory>();
            _ = mockRunSettingsTemplateReplacementFactory.Setup(
                runSettingsTemplateReplacementsFactory =>
                runSettingsTemplateReplacementsFactory.Create(coverageProject, "FccTestAdapterPath")
            ).Returns(runSettingsTemplateReplacements);

            var result = await this.templatedRunSettingsService.GenerateAsync(
                coverageProjects, "SolutionDirectory", "FccTestAdapterPath"
            );

            mockRunSettingsTemplate.Verify(
                runSettingsTemplate => runSettingsTemplate.ReplaceTemplate(
                    "<MockRunSettingsTemplate/>",
                    runSettingsTemplateReplacements, isDotNetFramework)
            );

        }

        [Test]
        public async Task Should_Create_Run_Settings_From_Configured_Custom_Template_If_Available_Async()
        {
            var mockCustomRunSettingsTemplateProvider = this.autoMocker.GetMock<ICustomRunSettingsTemplateProvider>();
            _ = mockCustomRunSettingsTemplateProvider.Setup(
                customRunSettingsTemplateProvider =>
                customRunSettingsTemplateProvider.Provide(@"C:\SomeProject", "SolutionDirectory")
            ).Returns(new CustomRunSettingsTemplateDetails { Path = "Custom path", Template = "<CustomTemplate/>" });

            var runSettingsTemplateReplacements = this.SetupReplacements();

            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFile).Returns(@"C:\SomeProject\SomeProject.csproj");
            var coverageProject = mockCoverageProject.Object;
            var coverageProjects = new List<ICoverageProject> { coverageProject };

            var mockRunSettingsTemplate = this.autoMocker.GetMock<IRunSettingsTemplate>();
            _ = mockRunSettingsTemplate.Setup(runSettingsTemplate => runSettingsTemplate.ConfigureCustom("<CustomTemplate/>"))
                .Returns("<ConfiguredCustom/>");

            var result = await this.templatedRunSettingsService.GenerateAsync(
                coverageProjects, "SolutionDirectory", "FccTestAdapterPath"
            );


            mockRunSettingsTemplate.Verify(
                runSettingsTemplate => runSettingsTemplate.ReplaceTemplate(
                    "<ConfiguredCustom/>",
                    runSettingsTemplateReplacements, It.IsAny<bool>())
            );
        }

        [Test]
        public async Task Should_Return_ExceptionReason_Result_If_Throws_Creating_RunSettings_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFile).Returns(@"C:\SomeProject\SomeProject.csproj");
            var coverageProject = mockCoverageProject.Object;
            var coverageProjects = new List<ICoverageProject> { coverageProject };

            var exception = new Exception("The message");
            _ = this.SetupICustomRunSettingsTemplateProviderAllIsAny().Throws(exception);

            var result = await this.templatedRunSettingsService.GenerateAsync(
                coverageProjects, "SolutionDirectory", "FccTestAdapterPath"
            );
            var exceptionReason = result.ExceptionReason;

            Assert.Multiple(() =>
            {
                Assert.That(exceptionReason.Exception, Is.SameAs(exception));
                Assert.That(exceptionReason.Reason, Is.EqualTo("Exception generating runsettings from template"));
            });
        }

        [Test]
        public async Task Should_Write_Generated_RunSettings_Async()
        {
            this.SetupReplaceResult(new TemplateReplaceResult { Replaced = "RunSettings" });

            var coverageProjects = this.CreateCoverageProjectsSingle();
            _ = await this.templatedRunSettingsService.GenerateAsync(coverageProjects, "SolutionDirectory", "FccTestAdapterPath");

            var coverageProjectRunSettings = this.GetWriteProjectsRunSettingsAsyncArgument().Single();

            Assert.Multiple(() =>
            {
                Assert.That(coverageProjectRunSettings.RunSettings, Is.EqualTo("RunSettings"));
                Assert.That(coverageProjectRunSettings.CoverageProject, Is.EqualTo(coverageProjects.Single()));
            });
        }

        [Test]
        public async Task Should_Return_ExceptionReason_Result_If_Throws_Writing_Generated_RunSettings_Async()
        {
            this.SetupReplaceResult(new TemplateReplaceResult { Replaced = "RunSettings" });

            var exception = new Exception();
            var mockProjectRunSettingsGenerator = this.autoMocker.GetMock<IProjectRunSettingsGenerator>();
            _ = mockProjectRunSettingsGenerator.Setup(
                projectRunSettingsGenerator =>
                projectRunSettingsGenerator.WriteProjectsRunSettingsAsync(It.IsAny<IEnumerable<ICoverageProjectRunSettings>>())
            ).ThrowsAsync(exception);

            var coverageProjects = this.CreateCoverageProjectsSingle();
            var result = await this.templatedRunSettingsService.GenerateAsync(
                coverageProjects, "SolutionDirectory", "FccTestAdapterPath"
            );
            var exceptionReason = result.ExceptionReason;

            Assert.Multiple(() =>
            {
                Assert.That(exceptionReason.Exception, Is.SameAs(exception));
                Assert.That(exceptionReason.Reason, Is.EqualTo("Exception writing templated runsettings"));
            });
        }

        [Test]
        public async Task Should_Return_A_Result_With_No_ExceptionReason_When_No_Exception_Async()
        {
            var mockCoverageProject1 = new Mock<ICoverageProject>();
            _ = mockCoverageProject1.Setup(cp => cp.ProjectFile).Returns(@"C:\SomeProject\SomeProject.csproj");
            var mockCoverageProject2 = new Mock<ICoverageProject>();
            _ = mockCoverageProject2.Setup(cp => cp.ProjectFile).Returns(@"C:\SomeProject2\SomeProject2.csproj");
            var coverageProjects = new List<ICoverageProject>
            {
                mockCoverageProject1.Object,
                mockCoverageProject2.Object
            };

            var mockCustomRunSettingsTemplateProvider = this.autoMocker.GetMock<ICustomRunSettingsTemplateProvider>();
            _ = mockCustomRunSettingsTemplateProvider.SetupSequence(
                customRunSettingsTemplateProvider => customRunSettingsTemplateProvider.Provide(It.IsAny<string>(), It.IsAny<string>())
                ).Returns(new CustomRunSettingsTemplateDetails { Path = "Custom template path" })
                .Returns((CustomRunSettingsTemplateDetails)null);

            var mockRunSettingsTemplate = this.autoMocker.GetMock<IRunSettingsTemplate>();
            _ = mockRunSettingsTemplate.SetupSequence(
                runSettingsTemplate =>
                runSettingsTemplate.ReplaceTemplate(It.IsAny<string>(), It.IsAny<IRunSettingsTemplateReplacements>(), It.IsAny<bool>())
            ).Returns(
                new TemplateReplaceResult
                {
                    Replaced = "RunSettings1",
                    ReplacedTestAdapter = false
                }
            ).Returns(
                new TemplateReplaceResult
                {
                    Replaced = "RunSettings2",
                    ReplacedTestAdapter = true
                }
            );

            var result = await this.templatedRunSettingsService.GenerateAsync(
                coverageProjects, "SolutionDirectory", "FccTestAdapterPath"
            );

            Assert.Multiple(() =>
            {
                Assert.That(result.ExceptionReason, Is.Null);
                Assert.That(result.CustomTemplatePaths, Is.EqualTo(new List<string> { "Custom template path" }));
                Assert.That(result.CoverageProjectsWithFCCMsTestAdapter, Is.EqualTo(new List<ICoverageProject> { coverageProjects[1] }));
            });
        }

        [Test]
        public async Task Clean_Up_Should_Remove_Generated_Project_RunSettings_Async()
        {
            var coverageProjects = new List<ICoverageProject> { new Mock<ICoverageProject>().Object };

            await this.templatedRunSettingsService.CleanUpAsync(coverageProjects);

            this.autoMocker.Verify<IProjectRunSettingsGenerator>(
                projectRunSettingsGenerator => projectRunSettingsGenerator.RemoveGeneratedProjectSettingsAsync(coverageProjects)
            );
        }

        private Moq.Language.Flow.ISetup<ICustomRunSettingsTemplateProvider, CustomRunSettingsTemplateDetails> SetupICustomRunSettingsTemplateProviderAllIsAny()
        {
            var mockCustomRunSettingsTemplateProvider = this.autoMocker.GetMock<ICustomRunSettingsTemplateProvider>();
            return mockCustomRunSettingsTemplateProvider.Setup(customRunSettingsTemplateProvider =>
                customRunSettingsTemplateProvider.Provide(It.IsAny<string>(), It.IsAny<string>())
            );
        }

        private List<ICoverageProject> CreateCoverageProjectsSingle()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFile).Returns(@"C:\SomeProject\SomeProject.csproj");
            var coverageProject = mockCoverageProject.Object;
            return new List<ICoverageProject> { coverageProject };
        }

        private void SetupReplaceResult(ITemplateReplacementResult templateReplacementResult)
        {
            var mockRunSettingsTemplate = this.autoMocker.GetMock<IRunSettingsTemplate>();
            _ = mockRunSettingsTemplate.Setup(
                runSettingsTemplate =>
                runSettingsTemplate.ReplaceTemplate(It.IsAny<string>(), It.IsAny<IRunSettingsTemplateReplacements>(), It.IsAny<bool>())
            ).Returns(templateReplacementResult);
        }

        private IEnumerable<ICoverageProjectRunSettings> GetWriteProjectsRunSettingsAsyncArgument()
        {
            var mockProjectRunSettingsGenerator = this.autoMocker.GetMock<IProjectRunSettingsGenerator>();
            return mockProjectRunSettingsGenerator.Invocations
                .Single(invocation => invocation.Method.Name == nameof(IProjectRunSettingsGenerator.WriteProjectsRunSettingsAsync))
                .Arguments[0] as IEnumerable<ICoverageProjectRunSettings>;
        }

        private IRunSettingsTemplateReplacements SetupReplacements()
        {
            var runSettingsTemplateReplacements = new RunSettingsTemplateReplacements();
            var mockRunSettingsTemplateReplacementFactory = this.autoMocker.GetMock<IRunSettingsTemplateReplacementsFactory>();
            _ = mockRunSettingsTemplateReplacementFactory.Setup(
                runSettingsTemplateReplacementsFactory =>
                runSettingsTemplateReplacementsFactory.Create(It.IsAny<ICoverageProject>(), It.IsAny<string>())
            ).Returns(runSettingsTemplateReplacements);
            return runSettingsTemplateReplacements;
        }
    }

}
