using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using Moq;
using NUnit.Framework;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Options;
using System.Linq;

namespace FineCodeCoverageTests.OldStyleCoverage_Tests
{
    public class OldStyleCoverage_Tests
    {
        private AutoMoqer mocker;
        private OldStyleCoverage oldStyleCoverage;
        private CancellationToken vsLinkedCancellationToken;
        private const string DisabledProjectName = "DisabledProject";
        private const string EnabledProjectName = "EnabledProject";
        private TimeSpan coverageDuration = new TimeSpan(0, 1, 2, 3, 4);
        private const string coverageToolName = "ACoverageTool";
        private void CollectCoverage(params ICoverageProject[] coverageProjects)
        {
            mocker.GetMock<ICoverageUtilManager>().Setup(
                coverageUtilManager => coverageUtilManager.CoverageToolName(It.IsAny<ICoverageProject>())
            ).Returns(coverageToolName);

            mocker.GetMock<IExecutionTimer>().Setup(
                executionTimer => executionTimer.TimeAsync(It.IsAny<Func<Task>>())
            ).Callback<Func<Task>>(task => task()).ReturnsAsync(coverageDuration);

            mocker.Setup<IFCCEngine>(
                fccEngine => fccEngine.RunCancellableCoverageTask(
                    It.IsAny<Func<CancellationToken, Task<List<CoverageLine>>>>(), null
                )
            )
            .Callback<Func<CancellationToken, Task<List<CoverageLine>>>, Action>(
                (reportResultProvider, _) => reportResultProvider(vsLinkedCancellationToken)
            );

            oldStyleCoverage.CollectCoverage(() =>
            {
                return Task.FromResult(coverageProjects.ToList());
            });
        }
        private (Mock<ICoverageProject> mockDisabledCoverageProject, Mock<ICoverageProject> mockEnabledCoverageProject) CollectCoverageEnabledAndDisabled(Action<Mock<ICoverageProject>> setUpEnabled = null)
        {
            var mockDisabledProject = SetUpDisabledProject();

            var mockEnabledProject = SetupCoverageProject(EnabledProjectName);
            mockEnabledProject.Setup(
                    coverageProject => coverageProject.PrepareForCoverageAsync(vsLinkedCancellationToken, true)
                ).ReturnsAsync(
                    new CoverageProjectFileSynchronizationDetails
                    {
                        Logs = Enumerable.Empty<string>().ToList()
                    }
                );

            if (setUpEnabled != null)
            {
                setUpEnabled(mockEnabledProject);
            }

            CollectCoverage(
                mockDisabledProject.Object,
                mockEnabledProject.Object
            );

            return (mockDisabledProject, mockEnabledProject);
        }

        private Mock<ICoverageProject> SetupCoverageProject(string projectName, bool enabled = true, string projectFile = "a.csproj")
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            var mockCoveragProjectSettings = new Mock<IAppOptions>();
            mockCoveragProjectSettings.SetupGet(settings => settings.Enabled).Returns(enabled);
            mockCoverageProject.SetupGet(coverageProject => coverageProject.Settings).Returns(mockCoveragProjectSettings.Object);
            mockCoverageProject.SetupGet(coverageProject => coverageProject.ProjectFile).Returns(projectFile);
            mockCoverageProject.SetupGet(coverageProject => coverageProject.ProjectName).Returns(projectName);
            mockCoverageProject.SetupGet(coverageProject => coverageProject.CoverageOutputFile).Returns($"{projectName}.coveroutput");
            return mockCoverageProject;
        }

        private Mock<ICoverageProject> SetUpDisabledProject()
        {
            return SetupCoverageProject(DisabledProjectName, false);
        }

        [SetUp]
        public void SetUp()
        {
            vsLinkedCancellationToken = CancellationToken.None;
            mocker = new AutoMoqer();
            oldStyleCoverage = mocker.Create<OldStyleCoverage>();
        }

        [Test]
        public void Should_FCCEngine_StopCoverage_When_StopCoverage()
        {
            oldStyleCoverage.StopCoverage();

            mocker.Verify<IFCCEngine>(fccEngine => fccEngine.StopCoverage());
        }

        [Test]
        public void Should_RunCancellableCoverageTask_When_CollectCoverage()
        {
            oldStyleCoverage.CollectCoverage(() =>
            {
                return null;
            });

            mocker.Verify<IFCCEngine>(
                fccEngine => fccEngine.RunCancellableCoverageTask(It.IsAny<Func<CancellationToken, Task<List<CoverageLine>>>>(), null)
            );
        }

        [Test]
        public void Should_Log_Is_Starting_Coverage_When_CollectCoverage()
        {
            CollectCoverage();

            mocker.Verify<ILogger>(logger => logger.Log(CoverageStatus.Start.Message()));
            mocker.GetMock<IEventAggregator>().AssertLogToolWindowLinkShowFCCOutputPane("Starting coverage - full details in ", MessageContext.CoverageStart);
        }

        [Test]
        public void Should_SetProjectCoverageOutputFolder_Before_Preparing_Project_For_Coverage()
        {
            var preparedForCoverage = false;
            var setProjectCoverageOutputFolder = false;

            var mockEnabledCoverageProject = SetupCoverageProject("Enabled");
            mockEnabledCoverageProject.Setup(
                coverageProject => coverageProject.PrepareForCoverageAsync(
                    It.IsAny<CancellationToken>(),
                    true)
            ).Callback(() =>
            {
                preparedForCoverage = true;
                Assert.IsTrue(setProjectCoverageOutputFolder);
            });
            var enabledCoverageProject = mockEnabledCoverageProject.Object;

            
            mocker.Setup<ICoverageToolOutputManager>(
                coverageToolOutputManager => coverageToolOutputManager.SetProjectCoverageOutputFolder(
                    new List<ICoverageProject> { enabledCoverageProject }
                )
            ).Callback(() => setProjectCoverageOutputFolder = true);

            
            CollectCoverage(enabledCoverageProject);

            Assert.IsTrue(preparedForCoverage);
        }

        [Test]
        public void Should_Log_Coverage_Project_Disabled_For_Disabled_Coverage_Projects_When_CollectCoverage()
        {
            CollectCoverage(SetUpDisabledProject().Object);

            mocker.Verify<ILogger>(logger => logger.Log("DisabledProject disabled."));
            mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog("DisabledProject disabled.", MessageContext.Info);
        }

        [Test]
        public void Should_Prepare_Suitable_Coverage_Projects_When_CollectCoverage()
        {
            var (mockDisabledCoverageProject, mockEnabledCoverageProject) = CollectCoverageEnabledAndDisabled();

            mockDisabledCoverageProject.Verify(
                coverageProject => coverageProject.PrepareForCoverageAsync(It.IsAny<CancellationToken>(), It.IsAny<bool>()),
                Times.Never()
            );

            mockEnabledCoverageProject.Verify(
                coverageProject => coverageProject.PrepareForCoverageAsync(vsLinkedCancellationToken, true)
            );
        }

        [Test]
        public void Should_Log_File_Sync_Details_For_Suitable_Coverage_Projects()
        {
            var fileSynchronizationDuration = new TimeSpan(0, 1, 2, 3);
            CollectCoverageEnabledAndDisabled(
                mockEnabledCoverageProject => mockEnabledCoverageProject.Setup(
                    coverageProject => coverageProject.PrepareForCoverageAsync(vsLinkedCancellationToken, true)
                ).ReturnsAsync(new CoverageProjectFileSynchronizationDetails
                {
                    Duration = fileSynchronizationDuration,
                    Logs = new List<string> { "Log1", "Log2" }
                })
            );

            var durationHoursMinutesSeconds = fileSynchronizationDuration.ToStringHoursMinutesSeconds();
            var fileSynchronizationLog = $"File synchronization duration : {durationHoursMinutesSeconds}";
            IEnumerable<string> expectedLogs = new List<string> { fileSynchronizationLog, "Log1", "Log2" };
            mocker.Verify<ILogger>(logger => logger.Log(expectedLogs));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Send_ToolWindow_TaskCompleted_File_Sync_LogMessage_For_For_Suitable_Coverage_Projects(bool singleFileSyncLog)
        {
            var fileSynchronizationDuration = new TimeSpan(0, 1, 2, 3);
            CollectCoverageEnabledAndDisabled(
                mockEnabledCoverageProject => mockEnabledCoverageProject.Setup(
                    coverageProject => coverageProject.PrepareForCoverageAsync(vsLinkedCancellationToken, true)
                ).ReturnsAsync(new CoverageProjectFileSynchronizationDetails
                {
                    Duration = fileSynchronizationDuration,
                    Logs = singleFileSyncLog ? new List<string> { "Log1"} : new List<string> { "Log1", "Log2" }
                })
            );
            
            var durationHoursMinutesSeconds = fileSynchronizationDuration.ToStringHoursMinutesSeconds();
            var itemOrItems = singleFileSyncLog ? "item" : "items";
            var logCount = singleFileSyncLog ? 1 : 2;
            var expectedMessage  = $"File synchronization {logCount} {itemOrItems}, duration : {durationHoursMinutesSeconds}";

            mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog(expectedMessage, MessageContext.TaskCompleted);
        }

        [Test]
        public void Should_Log_Collecting_Coverage_For_Each_Suitable_Coverage_Project_When_CollectCoverage()
        {
            CollectCoverageEnabledAndDisabled();

            mocker.Verify<ILogger>(logger => logger.Log($"Run {coverageToolName} ({EnabledProjectName})"));
            mocker.Verify<ILogger>(logger => logger.Log($"Run ACoverageTool ({DisabledProjectName})"), Times.Never());
           
        }

        [Test]
        public void Should_Send_ToolWindow_CoverageToolStart_LogMessage_For_Each_Suitable_Coverage_Project_When_CollectCoverage()
        {
            CollectCoverageEnabledAndDisabled();

            var mockEventAggregator = mocker.GetMock<IEventAggregator>();
            mockEventAggregator.AssertSimpleSingleLog($"Run ACoverageTool ({EnabledProjectName})", MessageContext.CoverageToolStart);

            new List<string> { DisabledProjectName, DisabledProjectName }.ForEach(unsuitableProjectName =>
            {
                mockEventAggregator.AssertHasSimpleLogMessage(false, $"Run ACoverageTool ({unsuitableProjectName})", MessageContext.CoverageToolStart);
            });
        }

        [Test]
        public void Should_Run_Coverage_For_Each_Suitable_Coverage_Project_When_CollectCoverage()
        {
            var mockCoverageUtilManager = mocker.GetMock<ICoverageUtilManager>();
            
            CollectCoverageEnabledAndDisabled(enabledMockCoverageProject =>
            {
                mockCoverageUtilManager.Setup(coverageUtilManager => coverageUtilManager.RunCoverageAsync(enabledMockCoverageProject.Object, vsLinkedCancellationToken));
            });

            mockCoverageUtilManager.VerifyAll();
            mockCoverageUtilManager.VerifyNoOtherCalls();
        }

        [Test]
        public void Should_Log_When_Coverage_Completes_For_Each_Suitable_CoverageProject_WithDuration_When_CollectCoverage()
        {
            CollectCoverageEnabledAndDisabled();

            void VerifyLogsOrNot(string projectName, bool logs)
            {
                var expectedDurationMessage = $"Completed coverage for ({projectName}) : {coverageDuration.ToStringHoursMinutesSeconds()}";

                mocker.Verify<ILogger>(logger => logger.Log(expectedDurationMessage), logs ? Times.Once() : Times.Never());
            }

            VerifyLogsOrNot(EnabledProjectName, true);
            VerifyLogsOrNot(DisabledProjectName, false);
        }

        [Test]
        public void Should_Send_CoverageToolCompleted_Tool_Window_LogMessage_When_Coverage_Completes_For_Each_Suitable_CoverageProject_WithDuration_When_CollectCoverage()
        {
            CollectCoverageEnabledAndDisabled();

            void VerifySendsMessageOrNot(string projectName, bool logs)
            {
                var expectedDurationMessage = $"Completed coverage for ({projectName}) : {coverageDuration.ToStringHoursMinutesSeconds()}";
                mocker.GetMock<IEventAggregator>()
                .   AssertHasSimpleLogMessage(logs,expectedDurationMessage, MessageContext.CoverageToolCompleted);
            }

            VerifySendsMessageOrNot(EnabledProjectName, true);
            VerifySendsMessageOrNot(DisabledProjectName, false);
        }

        [Test]
        public void Should_RunAndProcessReport_With_CoverageOutputFile_Of_Each_Suitable_CoverageProject_When_CollectCoverage()
        {
            CollectCoverageEnabledAndDisabled();

            mocker.Verify<IFCCEngine>(fccEngine => fccEngine.RunAndProcessReport(new string[] { $"{EnabledProjectName}.coveroutput" }, vsLinkedCancellationToken));
        }
    }
}