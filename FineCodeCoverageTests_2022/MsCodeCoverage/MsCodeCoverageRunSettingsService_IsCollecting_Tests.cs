namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    internal class MsCodeCoverageRunSettingsService_StopCoverage_Test
    {
        [Test]
        public void Should_StopCoverage_On_FCCEngine()
        {
            var autoMocker = new AutoMoqer();

            var mockToolFolder = autoMocker.GetMock<IToolFolder>();
            _ = mockToolFolder.Setup(tf => tf.EnsureUnzipped(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZipDetails>(), It.IsAny<CancellationToken>())).Returns("ZipDestination");

            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();
            var mockFccEngine = new Mock<IFCCEngine>();

            msCodeCoverageRunSettingsService.Initialize(null, mockFccEngine.Object, CancellationToken.None);

            msCodeCoverageRunSettingsService.StopCoverage();
            mockFccEngine.Verify(fccEngine => fccEngine.StopCoverage());
        }
    }

    internal class UserRunSettingsAnalysisResult : IUserRunSettingsAnalysisResult
    {
        public UserRunSettingsAnalysisResult(bool suitable, bool specifiedMsCodeCoverage)
        {
            this.Suitable = suitable;
            this.SpecifiedMsCodeCoverage = specifiedMsCodeCoverage;
        }
        public UserRunSettingsAnalysisResult() { }

        public bool Suitable { get; set; }

        public bool SpecifiedMsCodeCoverage { get; set; }

        public List<ICoverageProject> ProjectsWithFCCMsTestAdapter { get; set; } = new List<ICoverageProject>();
    }

    internal class MsCodeCoverageRunSettingsService_IsCollecting_Tests
    {
        private AutoMoqer autoMocker;
        private MsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        private const string SolutionDirectory = "SolutionDirectory";

        private class ExceptionReason : IExceptionReason
        {
            public Exception Exception { get; set; }

            public string Reason { get; set; }
        }
        private class ProjectRunSettingsFromTemplateResult : IProjectRunSettingsFromTemplateResult
        {
            public IExceptionReason ExceptionReason { get; set; }

            public List<string> CustomTemplatePaths { get; set; } = new List<string>();

            public List<ICoverageProject> CoverageProjectsWithFCCMsTestAdapter { get; set; } = new List<ICoverageProject>();
        }

        [SetUp]
        public void SetupSut()
        {
            this.autoMocker = new AutoMoqer();
            this.msCodeCoverageRunSettingsService = this.autoMocker.Create<MsCodeCoverageRunSettingsService>();
            this.msCodeCoverageRunSettingsService.threadHelper = new TestThreadHelper();
            this.SetupAppOptionsProvider(RunMsCodeCoverage.Yes);
        }

        [Test]
        public async Task Should_Not_Be_Collecting_If_RunMsCodeCoverage_No_Async()
        {
            this.SetupAppOptionsProvider(RunMsCodeCoverage.No);
            var testOperation = this.SetUpTestOperation(new List<ICoverageProject> { });
            var collectionStatus = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            Assert.That(collectionStatus, Is.EqualTo(MsCodeCoverageCollectionStatus.NotCollecting));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Try_Analyse_Projects_With_Runsettings_Async(bool useMsCodeCoverageOption)
        {
            var runMsCodeCoverage = useMsCodeCoverageOption ? RunMsCodeCoverage.Yes : RunMsCodeCoverage.IfInRunSettings;
            this.SetupAppOptionsProvider(runMsCodeCoverage);

            var fccMsTestAdapterPath = this.InitializeFCCMsTestAdapterPath();

            var coverageProjectWithRunSettings = this.CreateCoverageProject(".runsettings");
            var templatedCoverageProject = this.CreateCoverageProject(null);
            var coverageProjects = new List<ICoverageProject> { coverageProjectWithRunSettings, templatedCoverageProject };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            _ = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            this.autoMocker.Verify<IUserRunSettingsService>(
                userRunSettingsService => userRunSettingsService.Analyse(
                    new List<ICoverageProject> { coverageProjectWithRunSettings },
                    useMsCodeCoverageOption,
                    fccMsTestAdapterPath)
                );

        }

        [Test] // in case shutdown visual studio before normal clean up operation
        public async Task Should_CleanUp_Projects_With_RunSettings_First_Async()
        {
            var coverageProjectWithRunSettings = this.CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject> { coverageProjectWithRunSettings, this.CreateCoverageProject(null) };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            var cleanedUp = false;
            var mockUserRunSettingsService = this.autoMocker.GetMock<IUserRunSettingsService>();
            _ = mockUserRunSettingsService.Setup(
                userRunSettingsService => userRunSettingsService.Analyse(
                    It.IsAny<IEnumerable<ICoverageProject>>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()
                )
            ).Callback(() => Assert.That(cleanedUp, Is.True));

            var mockTemplatedRunSettingsService = this.autoMocker.GetMock<ITemplatedRunSettingsService>();
            _ = mockTemplatedRunSettingsService.Setup(
                templatedRunSettingsService =>
                templatedRunSettingsService.CleanUpAsync(new List<ICoverageProject> { coverageProjectWithRunSettings })
            ).Callback(() => cleanedUp = true);
            _ = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            mockUserRunSettingsService.VerifyAll();
        }

        [Test]
        public async Task Should_Log_Exception_From_UserRunSettingsService_Analyse_Async()
        {
            var exception = new Exception("Msg");
            _ = await this.Throw_Exception_From_UserRunSettingsService_Analyse_Async(exception);
            this.VerifyLogException("Exception analysing runsettings files", exception);
        }

        [Test]
        public async Task Should_Have_Status_Error_When_Exception_From_UserRunSettingsService_Analyse_Async()
        {
            var exception = new Exception("Msg");
            var status = await this.Throw_Exception_From_UserRunSettingsService_Analyse_Async(exception);
            Assert.That(status, Is.EqualTo(MsCodeCoverageCollectionStatus.Error));
        }

        [Test]
        public async Task Should_Prepare_Coverage_Projects_When_Suitable_Async()
        {
            this.SetupAppOptionsProvider(RunMsCodeCoverage.IfInRunSettings);

            var mockTemplatedCoverageProject = new Mock<ICoverageProject>();
            var mockCoverageProjects = new List<Mock<ICoverageProject>>
            {
                mockTemplatedCoverageProject,
                this.CreateMinimalMockRunSettingsCoverageProject()
            };
            var coverageProjects = mockCoverageProjects.Select(mockCoverageProject => mockCoverageProject.Object).ToList();
            var testOperation = this.SetUpTestOperation(coverageProjects);

            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));

            _ = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            this.autoMocker.Verify<ICoverageToolOutputManager>(coverageToolOutputManager => coverageToolOutputManager.SetProjectCoverageOutputFolder(coverageProjects));
            foreach (var mockCoverageProject in mockCoverageProjects)
            {
                mockCoverageProject.Verify(coverageProject => coverageProject.PrepareForCoverageAsync(CancellationToken.None, false));
            }
        }

        [Test]
        public async Task Should_Set_UserRunSettingsProjectDetailsLookup_For_IRunSettingsService_When_Suitable_Async()
        {
            this.SetupAppOptionsProvider(RunMsCodeCoverage.IfInRunSettings);

            var projectSettings = new Mock<IAppOptions>().Object;
            var excludedReferencedProjects = new List<string>();
            var includedReferencedProjects = new List<string>();
            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null),
                this.CreateCoverageProject(
                    ".runsettings",
                    projectSettings,
                    "OutputFolder",
                    "Test.dll",
                    excludedReferencedProjects,
                    includedReferencedProjects
                )
            };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));

            _ = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            var userRunSettingsProjectDetailsLookup = this.msCodeCoverageRunSettingsService.userRunSettingsProjectDetailsLookup;
            Assert.That(userRunSettingsProjectDetailsLookup, Has.Count.EqualTo(1));
            var userRunSettingsProjectDetails = userRunSettingsProjectDetailsLookup["Test.dll"];
            Assert.Multiple(() =>
            {
                Assert.That(userRunSettingsProjectDetails.Settings, Is.SameAs(projectSettings));
                Assert.That(userRunSettingsProjectDetails.ExcludedReferencedProjects, Is.SameAs(excludedReferencedProjects));
                Assert.That(userRunSettingsProjectDetails.IncludedReferencedProjects, Is.SameAs(includedReferencedProjects));
                Assert.That(userRunSettingsProjectDetails.CoverageOutputFolder, Is.EqualTo("OutputFolder"));
                Assert.That(userRunSettingsProjectDetails.TestDllFile, Is.EqualTo("Test.dll"));
            });
        }

        [Test]
        public async Task Should_Be_Collecting_When_Suitable_RunSettings_And_No_Templates_Async()
        {
            var status = await this.IsCollecting_With_Suitable_RunSettings_Only_Async();
            Assert.That(status, Is.EqualTo(MsCodeCoverageCollectionStatus.Collecting));
        }

        [Test]
        public async Task Should_Combined_Log_Collecting_With_RunSettings_When_Only_Suitable_RunSettings_Async()
        {
            var expectedMessage = "Ms code coverage collecting with user runsettings";
            _ = await this.IsCollecting_With_Suitable_RunSettings_Only_Async();
            this.autoMocker.Verify<ILogger>(l => l.Log(expectedMessage));
            var mockEventAggregator = this.autoMocker.GetMock<IEventAggregator>();
            mockEventAggregator.AssertSimpleSingleLog(expectedMessage, MessageContext.CoverageToolStart);
        }

        [Test]
        public async Task Should_Not_Be_Collecting_If_User_RunSettings_Are_Not_Suitable_Async()
        {
            var testOperation = this.SetUpTestOperation();
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult());

            var collectionStatus = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);
            Assert.That(collectionStatus, Is.EqualTo(MsCodeCoverageCollectionStatus.NotCollecting));
        }

        [Test]
        public Task Should_Generate_RunSettings_From_Templates_When_MsCodeCoverage_Option_is_True_Async() =>
            this.GenerateRunSettingsFromTemplateAsync(true, false);

        [Test]
        public Task Should_Generate_RunSettings_From_Templates_When_RunSettings_SpecifiedMsCodeCoverage_Async() =>
            this.GenerateRunSettingsFromTemplateAsync(false, true);

        [Test]
        public async Task Should_Combined_Log_CoverageStart_And_Ms_CoverageToolStart_Async()
        {
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, true));

            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null)

            };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            _ = this.SetupTemplatedRunSettingsServiceGenerateAsyncAllIsAny().ReturnsAsync(
                new ProjectRunSettingsFromTemplateResult { }
            );


            _ = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            var mockEventAggregator = this.autoMocker.GetMock<IEventAggregator>();
            var expectedStartingCoverage = "Starting coverage";
            var expectedMsCodeCoverageCollecting = "Ms code coverage collecting";
            this.autoMocker.Verify<ILogger>(logger => logger.Log(expectedStartingCoverage));
            this.autoMocker.Verify<ILogger>(logger => logger.Log(expectedMsCodeCoverageCollecting));
            mockEventAggregator.AssertSimpleSingleLog(expectedStartingCoverage, MessageContext.CoverageStart);
            mockEventAggregator.AssertSimpleSingleLog(expectedMsCodeCoverageCollecting, MessageContext.CoverageToolStart);

        }

        [Test]
        public Task Should_Combined_Log_With_Custom_Template_Paths_When_Successfully_Generate_RunSettings_From_Templates_Async() =>
            this.Successful_RunSettings_From_Templates_CombinedLog_Test_Async(
                new List<string> { "Custom path 1", "Custom path 2", "Custom path 2" },
                new List<string> { "Ms code coverage - custom template paths", "Custom path 1", "Custom path 2" }
            );

        [Test]
        public async Task Should_Combined_Log_Exception_From_Generate_RunSettings_From_Templates_Async()
        {
            var exception = new Exception("The message");
            _ = await this.ExceptionWhenGenerateRunSettingsFromTemplatesAsync(exception);

            this.VerifyLogException("The reason", exception);
        }

        [Test]
        public async Task Should_Have_Status_Error_When_Exception_From_Generate_RunSettings_From_Templates_Async()
        {
            var status = await this.ExceptionWhenGenerateRunSettingsFromTemplatesAsync(new Exception());
            Assert.That(status, Is.EqualTo(MsCodeCoverageCollectionStatus.Error));
        }

        [Test]
        public async Task Should_Not_Be_Collecting_When_Template_Projects_And_Do_Not_Ms_Collect_Async()
        {
            this.SetupAppOptionsProvider(RunMsCodeCoverage.IfInRunSettings);
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));

            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null)

            };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            var status = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);
            Assert.That(status, Is.EqualTo(MsCodeCoverageCollectionStatus.NotCollecting));
        }

        [Test]
        public async Task Should_Shim_Copy_From_RunSettingsProjects_And_Template_Projects_That_Require_It_Async()
        {
            var shimPath = this.InitializeShimPath();

            var runSettingsProjectsForShim = new List<ICoverageProject>
            {
                this.CreateCoverageProject(".runsettings")
            };
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(
                new UserRunSettingsAnalysisResult
                {
                    Suitable = true,
                    SpecifiedMsCodeCoverage = true,
                    ProjectsWithFCCMsTestAdapter = runSettingsProjectsForShim
                });

            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null)

            };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            var templateProjectsForShim = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null)
            };
            _ = this.SetupTemplatedRunSettingsServiceGenerateAsyncAllIsAny().ReturnsAsync(
                new ProjectRunSettingsFromTemplateResult
                {
                    CoverageProjectsWithFCCMsTestAdapter = templateProjectsForShim
                }
            );

            var status = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            var expectedCoverageProjectsForShimCopy = runSettingsProjectsForShim;
            expectedCoverageProjectsForShimCopy.AddRange(templateProjectsForShim);
            this.autoMocker.Verify<IShimCopier>(shimCopier => shimCopier.Copy(shimPath, expectedCoverageProjectsForShimCopy));
        }

        private Task<MsCodeCoverageCollectionStatus> IsCollecting_With_Suitable_RunSettings_Only_Async()
        {
            var testOperation = this.SetUpTestOperation();
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));
            return this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);
        }

        private async Task GenerateRunSettingsFromTemplateAsync(bool msCodeCoverageOptions, bool runSettingsSpecifiedMsCodeCoverage)
        {
            var runMsCodeCoverage = msCodeCoverageOptions ? RunMsCodeCoverage.Yes : RunMsCodeCoverage.IfInRunSettings;
            this.SetupAppOptionsProvider(runMsCodeCoverage);
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, runSettingsSpecifiedMsCodeCoverage));

            var fccMsTestAdapterPath = this.InitializeFCCMsTestAdapterPath();

            var templateCoverageProject = this.CreateCoverageProject(null);
            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateMinimalRunSettingsCoverageProject(),
                templateCoverageProject
            };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            var mockTemplatedRunSettingsService = this.autoMocker.GetMock<ITemplatedRunSettingsService>();
            _ = mockTemplatedRunSettingsService.Setup(templatedRunSettingsService => templatedRunSettingsService.GenerateAsync(
                      new List<ICoverageProject> { templateCoverageProject },
                      SolutionDirectory,
                      fccMsTestAdapterPath
                  )).ReturnsAsync(
                new ProjectRunSettingsFromTemplateResult()
            );

            _ = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            mockTemplatedRunSettingsService.VerifyAll();
        }

        private async Task Successful_RunSettings_From_Templates_CombinedLog_Test_Async(List<string> customTemplatePaths, List<string> expectedLoggerMessages)
        {
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, true));

            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null)

            };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            _ = this.SetupTemplatedRunSettingsServiceGenerateAsyncAllIsAny().ReturnsAsync(
                new ProjectRunSettingsFromTemplateResult { CustomTemplatePaths = customTemplatePaths }
            );


            _ = await this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            this.autoMocker.Verify<ILogger>(logger => logger.Log(expectedLoggerMessages));
        }

        private Task<MsCodeCoverageCollectionStatus> ExceptionWhenGenerateRunSettingsFromTemplatesAsync(Exception exception)
        {
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, true));

            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null)

            };
            var testOperation = this.SetUpTestOperation(coverageProjects);

            _ = this.SetupTemplatedRunSettingsServiceGenerateAsyncAllIsAny().ReturnsAsync(
                new ProjectRunSettingsFromTemplateResult
                {
                    ExceptionReason = new ExceptionReason
                    {
                        Exception = exception,
                        Reason = "The reason"
                    }
                }
            );

            return this.msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);
        }

        private Task<MsCodeCoverageCollectionStatus> Throw_Exception_From_UserRunSettingsService_Analyse_Async(Exception exception)
        {
            _ = this.SetupIUserRunSettingsServiceAnalyseAny().Throws(exception);
            return this.msCodeCoverageRunSettingsService.IsCollectingAsync(this.SetUpTestOperation());
        }

        private Moq.Language.Flow.ISetup<ITemplatedRunSettingsService, Task<IProjectRunSettingsFromTemplateResult>> SetupTemplatedRunSettingsServiceGenerateAsyncAllIsAny()
        {
            var mockTemplatedRunSettingsService = this.autoMocker.GetMock<ITemplatedRunSettingsService>();
            return mockTemplatedRunSettingsService.Setup(templatedRunSettingsService => templatedRunSettingsService.GenerateAsync(
                    It.IsAny<IEnumerable<ICoverageProject>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ));
        }

        private Moq.Language.Flow.ISetup<IUserRunSettingsService, IUserRunSettingsAnalysisResult> SetupIUserRunSettingsServiceAnalyseAny()
        {
            var mockUserRunSettingsService = this.autoMocker.GetMock<IUserRunSettingsService>();
            return mockUserRunSettingsService.Setup(userRunSettingsService => userRunSettingsService.Analyse(
                It.IsAny<List<ICoverageProject>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()
            ));
        }

        private ITestOperation SetUpTestOperation(List<ICoverageProject> coverageProjects = null)
        {
            coverageProjects = coverageProjects ?? new List<ICoverageProject>();
            var mockTestOperation = new Mock<ITestOperation>();
            _ = mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);
            _ = mockTestOperation.Setup(testOperation => testOperation.SolutionDirectory).Returns(SolutionDirectory);
            return mockTestOperation.Object;
        }

        private void SetupAppOptionsProvider(RunMsCodeCoverage runMsCodeCoverage)
        {
            var mockAppOptionsProvider = this.autoMocker.GetMock<IAppOptionsProvider>();
            var mockOptions = new Mock<IAppOptions>();
            _ = mockOptions.Setup(options => options.RunMsCodeCoverage).Returns(runMsCodeCoverage);
            _ = mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(mockOptions.Object);
        }

        private void VerifyLogException(string reason, Exception exception)
        {
            this.autoMocker.Verify<ILogger>(l => l.Log(reason, exception.ToString()));
            var mockEventAggregator = this.autoMocker.GetMock<IEventAggregator>();
            mockEventAggregator.AssertSimpleSingleLog(reason, MessageContext.Error);
        }

        private string InitializeFCCMsTestAdapterPath()
        {
            this.InitializeZipDestination();
            return Path.Combine("ZipDestination", "build", "netstandard1.0");
        }

        private string InitializeShimPath()
        {
            this.InitializeZipDestination();
            return Path.Combine("ZipDestination", "build", "netstandard1.0", "CodeCoverage", "coreclr", "Microsoft.VisualStudio.CodeCoverage.Shim.dll");
        }

        private void InitializeZipDestination()
        {
            var mockToolFolder = this.autoMocker.GetMock<IToolFolder>();
            _ = mockToolFolder.Setup(tf => tf.EnsureUnzipped(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZipDetails>(), It.IsAny<CancellationToken>())).Returns("ZipDestination");
            this.msCodeCoverageRunSettingsService.Initialize(null, null, CancellationToken.None);
        }

        private ICoverageProject CreateCoverageProject(
            string runSettingsFile,
            IAppOptions settings = null,
            string coverageOutputFolder = "",
            string testDllFile = "",
            List<string> excludedReferencedProjects = null,
            List<string> includedReferencedProjects = null,
            string projectFile = ""
        )
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns(runSettingsFile);
            _ = mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns(coverageOutputFolder);
            _ = mockCoverageProject.Setup(cp => cp.TestDllFile).Returns(testDllFile);
            _ = mockCoverageProject.Setup(cp => cp.ExcludedReferencedProjects).Returns(excludedReferencedProjects);
            _ = mockCoverageProject.Setup(cp => cp.IncludedReferencedProjects).Returns(includedReferencedProjects);
            _ = mockCoverageProject.Setup(cp => cp.Settings).Returns(settings);
            _ = mockCoverageProject.Setup(cp => cp.ProjectFile).Returns(projectFile);
            return mockCoverageProject.Object;
        }

        private Mock<ICoverageProject> CreateMinimalMockRunSettingsCoverageProject()
        {
            var mockCoverageProjectWithRunSettings = new Mock<ICoverageProject>();
            _ = mockCoverageProjectWithRunSettings.Setup(cp => cp.RunSettingsFile).Returns(".runsettings");
            _ = mockCoverageProjectWithRunSettings.Setup(cp => cp.TestDllFile).Returns("Test.dll");
            return mockCoverageProjectWithRunSettings;
        }

        private ICoverageProject CreateMinimalRunSettingsCoverageProject() =>
            this.CreateMinimalMockRunSettingsCoverageProject().Object;
    }
}
