using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output.JsMessages.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core
{
    [Export(typeof(IOldStyleCoverage))]
    internal class OldStyleCoverage : IOldStyleCoverage
    {
        private readonly IFCCEngine fccEngine;
        private readonly IEventAggregator eventAggregator;
        private readonly ILogger logger;
        private readonly ICoverageToolOutputManager coverageOutputManager;
        private readonly ICoverageUtilManager coverageUtilManager;
        private readonly IExecutionTimer executionTimer;

        [ImportingConstructor]
        public OldStyleCoverage(
            IFCCEngine fccEngine,
            IEventAggregator eventAggregator,
            ILogger logger,
            ICoverageToolOutputManager coverageOutputManager,
            ICoverageUtilManager coverageUtilManager,
            IExecutionTimer executionTimer
        )
        {
            this.fccEngine = fccEngine;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
            this.coverageOutputManager = coverageOutputManager;
            this.coverageUtilManager = coverageUtilManager;
            this.executionTimer = executionTimer;
        }

        public void StopCoverage()
        {
            fccEngine.StopCoverage();
        }

        public void CollectCoverage(Func<Task<List<ICoverageProject>>> getCoverageProjects)
        {
            fccEngine.RunCancellableCoverageTask(async (vsShutdownLinkedCancellationToken) =>
            {
                LogStartingCoverage();

                var coverageProjects = await getCoverageProjects();
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

                coverageOutputManager.SetProjectCoverageOutputFolder(coverageProjects);

                var coverOutputFiles = await RunCoverageAsync(coverageProjects, vsShutdownLinkedCancellationToken);
                return fccEngine.RunAndProcessReport(coverOutputFiles, vsShutdownLinkedCancellationToken);
            }, null);
        }

        private async Task<string[]> RunCoverageAsync(List<ICoverageProject> coverageProjects, CancellationToken vsShutdownLinkedCancellationToken)
        {
            List<string> coverageOutputFiles = new List<string>();
            foreach (var coverageProject in coverageProjects)
            {
                var collectedCoverage = await TryCollectCoverageForProjectAsync(coverageProject, vsShutdownLinkedCancellationToken);
                if (collectedCoverage)
                {
                    coverageOutputFiles.Add(coverageProject.CoverageOutputFile);
                }
            }
            return coverageOutputFiles.ToArray();
        }

        private async Task<bool> TryCollectCoverageForProjectAsync(ICoverageProject coverageProject, CancellationToken vsShutdownLinkedCancellationToken)
        {
            var collectedCoverage = false;
            if (!coverageProject.Settings.Enabled)
            {
                LogCoverageProjectDisabled(coverageProject);
            }
            else
            {
                await CollectCoverageForProjectAsync(coverageProject, vsShutdownLinkedCancellationToken);
                collectedCoverage = true;
            }
            return collectedCoverage;
        }

        private async Task CollectCoverageForProjectAsync(ICoverageProject project, CancellationToken vsShutdownLinkedCancellationToken)
        {
            await PrepareProjectForCoverageAsync(project, vsShutdownLinkedCancellationToken);
            LogStartingCoverageForProject(project);

            var duration = await executionTimer.TimeAsync(
                async () => await coverageUtilManager.RunCoverageAsync(project, vsShutdownLinkedCancellationToken)
            );

            LogCompletedCoverageForProject(project, duration);
        }

        private async Task PrepareProjectForCoverageAsync(ICoverageProject project, CancellationToken vsShutdownLinkedCancellationToken)
        {
            var fileSynchronizationDetails = await project.PrepareForCoverageAsync(vsShutdownLinkedCancellationToken);
            LogFileSynchronizationDetails(fileSynchronizationDetails);
        }

        #region logging
        private void LogStartingCoverage()
        {
            eventAggregator.LogToolWindowLinkFCCOutputPane("Starting coverage - full details in ", MessageContext.CoverageStart);
            logger.Log(CoverageStatus.Start.Message());
        }

        private void LogCoverageProjectDisabled(ICoverageProject coverageProject)
        {
            var message = $"{coverageProject.ProjectName} disabled.";
            logger.Log(message);
            eventAggregator.SimpleLogToolWindow(message);
        }

        private void LogStartingCoverageForProject(ICoverageProject project)
        {
            var coverageTool = coverageUtilManager.CoverageToolName(project);
            var runCoverToolMessage = $"Run {coverageTool} ({project.ProjectName})";
            logger.Log(runCoverToolMessage);
            eventAggregator.SimpleLogToolWindow(runCoverToolMessage, MessageContext.CoverageToolStart);
        }

        private void LogFileSynchronizationDetails(CoverageProjectFileSynchronizationDetails fileSynchronizationDetails)
        {
            var logs = fileSynchronizationDetails.Logs;
            if (logs.Any())
            {
                var durationHoursMinutesSeconds = fileSynchronizationDetails.Duration.ToStringHoursMinutesSeconds();

                var itemOrItems = logs.Count == 1 ? "item" : "items";
                var fileSynchronizationLog = $"File synchronization {logs.Count} {itemOrItems}, duration : {durationHoursMinutesSeconds}";
                eventAggregator.SimpleLogToolWindow(fileSynchronizationLog, MessageContext.TaskCompleted);

                fileSynchronizationLog = $"File synchronization duration : {durationHoursMinutesSeconds}";
                logs = new string[] { fileSynchronizationLog }.Concat(logs).ToList();
                logger.Log(logs);

                
            }
        }

        private void LogCompletedCoverageForProject(ICoverageProject coverageProject, TimeSpan duration)
        {
            var durationMessage = $"Completed coverage for ({coverageProject.ProjectName}) : {duration.ToStringHoursMinutesSeconds()}";
            logger.Log(durationMessage);
            eventAggregator.SimpleLogToolWindow(durationMessage, MessageContext.CoverageToolCompleted);
        }
        #endregion
    }
}
