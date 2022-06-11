namespace FineCodeCoverageTests.OldStyleCoverage_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    public class OldStyleCoverage_Tests
    {
        private AutoMoqer mocker;
        private OldStyleCoverage oldStyleCoverage;
        private CancellationToken vsLinkedCancellationToken;
        private const string DisabledProjectName = "DisabledProject";
        private const string EnabledProjectName = "EnabledProject";
        private TimeSpan coverageDuration = new TimeSpan(0, 1, 2, 3, 4);
        private const string CoverageToolName = "ACoverageTool";

        private void CollectCoverage(params ICoverageProject[] coverageProjects)
        {
            _ = this.mocker.GetMock<ICoverageUtilManager>().Setup(
                coverageUtilManager => coverageUtilManager.CoverageToolName(It.IsAny<ICoverageProject>())
            ).Returns(CoverageToolName);

            _ = this.mocker.GetMock<IExecutionTimer>().Setup(
                executionTimer => executionTimer.TimeAsync(It.IsAny<Func<Task>>())
            ).Callback<Func<Task>>(task => task()).ReturnsAsync(this.coverageDuration);

            _ = this.mocker.Setup<IFCCEngine>(
                fccEngine => fccEngine.RunCancellableCoverageTask(
                    It.IsAny<Func<CancellationToken, Task<List<CoverageLine>>>>(), null
                )
            )
            .Callback<Func<CancellationToken, Task<List<CoverageLine>>>, Action>(
                (reportResultProvider, _) => reportResultProvider(this.vsLinkedCancellationToken)
            );

            this.oldStyleCoverage.CollectCoverage(() => Task.FromResult(coverageProjects.ToList()));
        }

        private (Mock<ICoverageProject> mockDisabledCoverageProject, Mock<ICoverageProject> mockEnabledCoverageProject) CollectCoverageEnabledAndDisabled(Action<Mock<ICoverageProject>> setUpEnabled = null)
        {
            var mockDisabledProject = this.SetUpDisabledProject();

            var mockEnabledProject = this.SetupCoverageProject(EnabledProjectName);
            _ = mockEnabledProject.Setup(
                    coverageProject => coverageProject.PrepareForCoverageAsync(this.vsLinkedCancellationToken, true)
            ).ReturnsAsync(
                new CoverageProjectFileSynchronizationDetails
                {
                    Logs = Enumerable.Empty<string>().ToList()
                }
            );

            setUpEnabled?.Invoke(mockEnabledProject);

            this.CollectCoverage(
                mockDisabledProject.Object,
                mockEnabledProject.Object
            );

            return (mockDisabledProject, mockEnabledProject);
        }

        private Mock<ICoverageProject> SetupCoverageProject(string projectName, bool enabled = true, string projectFile = "a.csproj")
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            var mockCoveragProjectSettings = new Mock<IAppOptions>();
            _ = mockCoveragProjectSettings.SetupGet(settings => settings.Enabled).Returns(enabled);
            _ = mockCoverageProject.SetupGet(coverageProject => coverageProject.Settings).Returns(mockCoveragProjectSettings.Object);
            _ = mockCoverageProject.SetupGet(coverageProject => coverageProject.ProjectFile).Returns(projectFile);
            _ = mockCoverageProject.SetupGet(coverageProject => coverageProject.ProjectName).Returns(projectName);
            _ = mockCoverageProject.SetupGet(coverageProject => coverageProject.CoverageOutputFile).Returns($"{projectName}.coveroutput");
            return mockCoverageProject;
        }

        private Mock<ICoverageProject> SetUpDisabledProject() => this.SetupCoverageProject(DisabledProjectName, false);

        [SetUp]
        public void SetUp()
        {
            this.vsLinkedCancellationToken = CancellationToken.None;
            this.mocker = new AutoMoqer();
            this.oldStyleCoverage = this.mocker.Create<OldStyleCoverage>();
        }

        [Test]
        public void Should_FCCEngine_StopCoverage_When_StopCoverage()
        {
            this.oldStyleCoverage.StopCoverage();

            this.mocker.Verify<IFCCEngine>(fccEngine => fccEngine.StopCoverage());
        }

        [Test]
        public void Should_RunCancellableCoverageTask_When_CollectCoverage()
        {
            this.oldStyleCoverage.CollectCoverage(() => Task.FromResult<List<ICoverageProject>>(null));

            this.mocker.Verify<IFCCEngine>(
                fccEngine => fccEngine.RunCancellableCoverageTask(
                    It.IsAny<Func<CancellationToken, Task<List<CoverageLine>>>>(), null
                )
            );
        }

        [Test]
        public void Should_Log_Is_Starting_Coverage_When_CollectCoverage()
        {
            this.CollectCoverage();

            this.mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Start.Message()));
            this.mocker.GetMock<IEventAggregator>().AssertLogToolWindowLinkShowFCCOutputPane(
                "Starting coverage - full details in ",
                MessageContext.CoverageStart
            );
        }

        [Test]
        public void Should_SetProjectCoverageOutputFolder_Before_Preparing_Project_For_Coverage()
        {
            var preparedForCoverage = false;
            var setProjectCoverageOutputFolder = false;

            var mockEnabledCoverageProject = this.SetupCoverageProject("Enabled");
            _ = mockEnabledCoverageProject.Setup(
                coverageProject => coverageProject.PrepareForCoverageAsync(
                    It.IsAny<CancellationToken>(),
                    true)
            ).Callback(() =>
            {
                preparedForCoverage = true;
                Assert.That(setProjectCoverageOutputFolder, Is.True);
            });
            var enabledCoverageProject = mockEnabledCoverageProject.Object;


            _ = this.mocker.Setup<ICoverageToolOutputManager>(
                coverageToolOutputManager => coverageToolOutputManager.SetProjectCoverageOutputFolder(
                    new List<ICoverageProject> { enabledCoverageProject }
                )
            ).Callback(() => setProjectCoverageOutputFolder = true);


            this.CollectCoverage(enabledCoverageProject);

            Assert.That(preparedForCoverage, Is.True);
        }

        [Test]
        public void Should_Log_Coverage_Project_Disabled_For_Disabled_Coverage_Projects_When_CollectCoverage()
        {
            this.CollectCoverage(this.SetUpDisabledProject().Object);

            this.mocker.Verify<ILogger>(logger => logger.Log("DisabledProject disabled."));
            this.mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("DisabledProject disabled.", MessageContext.Info);
        }

        [Test]
        public void Should_Prepare_Suitable_Coverage_Projects_When_CollectCoverage()
        {
            var (mockDisabledCoverageProject, mockEnabledCoverageProject) = this.CollectCoverageEnabledAndDisabled();

            mockDisabledCoverageProject.Verify(
                coverageProject => coverageProject.PrepareForCoverageAsync(It.IsAny<CancellationToken>(), It.IsAny<bool>()),
                Times.Never()
            );

            mockEnabledCoverageProject.Verify(
                coverageProject => coverageProject.PrepareForCoverageAsync(this.vsLinkedCancellationToken, true)
            );
        }

        [Test]
        public void Should_Log_File_Sync_Details_For_Suitable_Coverage_Projects()
        {
            var fileSynchronizationDuration = new TimeSpan(0, 1, 2, 3);
            _ = this.CollectCoverageEnabledAndDisabled(
                mockEnabledCoverageProject => mockEnabledCoverageProject.Setup(
                    coverageProject => coverageProject.PrepareForCoverageAsync(this.vsLinkedCancellationToken, true)
                ).ReturnsAsync(new CoverageProjectFileSynchronizationDetails
                {
                    Duration = fileSynchronizationDuration,
                    Logs = new List<string> { "Log1", "Log2" }
                })
            );

            var durationHoursMinutesSeconds = fileSynchronizationDuration.ToStringHoursMinutesSeconds();
            var fileSynchronizationLog = $"File synchronization duration : {durationHoursMinutesSeconds}";
            IEnumerable<string> expectedLogs = new List<string> { fileSynchronizationLog, "Log1", "Log2" };
            this.mocker.Verify<ILogger>(logger => logger.Log(expectedLogs));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Send_ToolWindow_TaskCompleted_File_Sync_LogMessage_For_For_Suitable_Coverage_Projects(bool singleFileSyncLog)
        {
            var fileSynchronizationDuration = new TimeSpan(0, 1, 2, 3);
            _ = this.CollectCoverageEnabledAndDisabled(
                mockEnabledCoverageProject => mockEnabledCoverageProject.Setup(
                    coverageProject => coverageProject.PrepareForCoverageAsync(this.vsLinkedCancellationToken, true)
                ).ReturnsAsync(new CoverageProjectFileSynchronizationDetails
                {
                    Duration = fileSynchronizationDuration,
                    Logs = singleFileSyncLog ? new List<string> { "Log1" } : new List<string> { "Log1", "Log2" }
                })
            );

            var durationHoursMinutesSeconds = fileSynchronizationDuration.ToStringHoursMinutesSeconds();
            var itemOrItems = singleFileSyncLog ? "item" : "items";
            var logCount = singleFileSyncLog ? 1 : 2;
            var expectedMessage = $"File synchronization {logCount} {itemOrItems}, duration : {durationHoursMinutesSeconds}";

            this.mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog(expectedMessage, MessageContext.TaskCompleted);
        }

        [Test]
        public void Should_Log_Collecting_Coverage_For_Each_Suitable_Coverage_Project_When_CollectCoverage()
        {
            _ = this.CollectCoverageEnabledAndDisabled();

            this.mocker.Verify<ILogger>(logger => logger.Log($"Run {CoverageToolName} ({EnabledProjectName})"));
            this.mocker.Verify<ILogger>(logger => logger.Log($"Run ACoverageTool ({DisabledProjectName})"), Times.Never());

        }

        [Test]
        public void Should_Send_ToolWindow_CoverageToolStart_LogMessage_For_Each_Suitable_Coverage_Project_When_CollectCoverage()
        {
            _ = this.CollectCoverageEnabledAndDisabled();

            var mockEventAggregator = this.mocker.GetMock<IEventAggregator>();
            mockEventAggregator.AssertSimpleSingleLog($"Run ACoverageTool ({EnabledProjectName})", MessageContext.CoverageToolStart);

            new List<string> { DisabledProjectName, DisabledProjectName }.ForEach(
                unsuitableProjectName => mockEventAggregator.AssertHasSimpleLogMessage(
                    false,
                    $"Run ACoverageTool ({unsuitableProjectName})",
                    MessageContext.CoverageToolStart
                )
            );
        }

        [Test]
        public void Should_Run_Coverage_For_Each_Suitable_Coverage_Project_When_CollectCoverage()
        {
            var mockCoverageUtilManager = this.mocker.GetMock<ICoverageUtilManager>();

            _ = this.CollectCoverageEnabledAndDisabled(
                enabledMockCoverageProject => mockCoverageUtilManager.Setup(
                    coverageUtilManager => coverageUtilManager.RunCoverageAsync(
                        enabledMockCoverageProject.Object,
                        this.vsLinkedCancellationToken)
                    )
            );

            mockCoverageUtilManager.VerifyAll();
            mockCoverageUtilManager.VerifyNoOtherCalls();
        }

        [Test]
        public void Should_Log_When_Coverage_Completes_For_Each_Suitable_CoverageProject_WithDuration_When_CollectCoverage()
        {
            _ = this.CollectCoverageEnabledAndDisabled();

            void VerifyLogsOrNot(string projectName, bool logs)
            {
                var expectedDurationMessage = $"Completed coverage for ({projectName}) : {this.coverageDuration.ToStringHoursMinutesSeconds()}";

                this.mocker.Verify<ILogger>(logger => logger.Log(expectedDurationMessage), logs ? Times.Once() : Times.Never());
            }

            VerifyLogsOrNot(EnabledProjectName, true);
            VerifyLogsOrNot(DisabledProjectName, false);
        }

        [Test]
        public void Should_Send_CoverageToolCompleted_Tool_Window_LogMessage_When_Coverage_Completes_For_Each_Suitable_CoverageProject_WithDuration_When_CollectCoverage()
        {
            _ = this.CollectCoverageEnabledAndDisabled();

            void VerifySendsMessageOrNot(string projectName, bool logs)
            {
                var expectedDurationMessage = $"Completed coverage for ({projectName}) : {this.coverageDuration.ToStringHoursMinutesSeconds()}";
                this.mocker.GetMock<IEventAggregator>()
                .AssertHasSimpleLogMessage(logs, expectedDurationMessage, MessageContext.CoverageToolCompleted);
            }

            VerifySendsMessageOrNot(EnabledProjectName, true);
            VerifySendsMessageOrNot(DisabledProjectName, false);
        }

        [Test]
        public void Should_RunAndProcessReport_With_CoverageOutputFile_Of_Each_Suitable_CoverageProject_When_CollectCoverage()
        {
            _ = this.CollectCoverageEnabledAndDisabled();

            this.mocker.Verify<IFCCEngine>(
                fccEngine => fccEngine.RunAndProcessReport(
                    new string[] { $"{EnabledProjectName}.coveroutput" },
                    this.vsLinkedCancellationToken
                )
            );
        }
    }
}
